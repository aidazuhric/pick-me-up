import 'dart:typed_data';

import 'package:file_selector/file_selector.dart';
import 'package:flutter/material.dart';
import 'package:printing/printing.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../utils/api_error_handler.dart';
import 'app_messages.dart';

class PdfReportView extends StatelessWidget {
  const PdfReportView({
    super.key,
    required this.document,
    this.emptyMessage = 'Choose a period, then press GENERATE.',
  });

  final Uint8List? document;
  final String emptyMessage;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final bytes = document;

    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: bytes == null
          ? Center(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.picture_as_pdf_outlined,
                        size: 36, color: AppColors.textMuted),
                    const SizedBox(height: AppSpacing.sm),
                    Text(
                      emptyMessage,
                      style: theme.textTheme.bodyMedium,
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            )
          : ClipRRect(
              borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
              child: PdfPreview(
                build: (_) async => bytes,
                allowPrinting: false,
                allowSharing: false,
                canChangePageFormat: false,
                canChangeOrientation: false,
                canDebug: false,
                useActions: false,
                padding: const EdgeInsets.all(AppSpacing.md),
              ),
            ),
    );
  }
}

class PdfReportActions extends StatelessWidget {
  const PdfReportActions({
    super.key,
    required this.document,
    required this.fileName,
  });

  final Uint8List? document;
  final String? fileName;

  Future<void> _download(BuildContext context) async {
    final bytes = document;
    if (bytes == null) return;

    try {
      final location = await getSaveLocation(
        suggestedName: fileName,
        acceptedTypeGroups: const [
          XTypeGroup(label: 'PDF', extensions: ['pdf']),
        ],
      );

      if (location == null) return;

      final file = XFile.fromData(
        bytes,
        mimeType: 'application/pdf',
        name: fileName,
      );
      await file.saveTo(location.path);

      if (!context.mounted) return;
      AppMessages.success(context, 'Saved to ${location.path}.');
    } catch (e) {
      if (!context.mounted) return;
      ApiErrorHandler.surface(context, e);
    }
  }

  Future<void> _print(BuildContext context) async {
    final bytes = document;
    if (bytes == null) return;

    try {
      await Printing.layoutPdf(
        name: fileName ?? 'report.pdf',
        onLayout: (_) async => bytes,
      );
    } catch (e) {
      if (!context.mounted) return;
      ApiErrorHandler.surface(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: OutlinedButton.icon(
            onPressed: document == null ? null : () => _download(context),
            icon: const Icon(Icons.download_outlined, size: 16),
            label: const Text('Download'),
          ),
        ),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: OutlinedButton.icon(
            onPressed: document == null ? null : () => _print(context),
            icon: const Icon(Icons.print_outlined, size: 16),
            label: const Text('Print'),
          ),
        ),
      ],
    );
  }
}
