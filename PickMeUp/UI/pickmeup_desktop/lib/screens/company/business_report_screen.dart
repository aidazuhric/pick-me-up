import 'dart:typed_data';

import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../providers/report_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/pdf_report_view.dart';

class BusinessReportScreen extends StatefulWidget {
  const BusinessReportScreen({super.key});

  @override
  State<BusinessReportScreen> createState() => _BusinessReportScreenState();
}

class _BusinessReportScreenState extends State<BusinessReportScreen> {
  final _provider = ReportProvider();

  late DateTime _from = _defaultReportStart();
  late DateTime _to = DateTime.now();

  Uint8List? _document;
  String? _fileName;
  bool _isGenerating = false;

  Future<void> _generate() async {
    if (_from.isAfter(_to)) {
      AppMessages.failure(
        context,
        'The start of the period must not be after its end.',
      );
      return;
    }

    setState(() => _isGenerating = true);

    try {
      final bytes = await _provider.companyBusiness(from: _from, to: _to);

      if (!mounted) return;

      setState(() {
        _document = bytes;
        _fileName = 'company-business-${_stamp(_from)}-${_stamp(_to)}.pdf';
        _isGenerating = false;
      });

      AppMessages.success(
        context,
        'Your business report is ready for ${Formats.day(_from)} to ${Formats.day(_to)}.',
      );
    } catch (e) {
      if (!mounted) return;
      setState(() => _isGenerating = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  Future<void> _pickDate({required bool isFrom}) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: isFrom ? _from : _to,
      firstDate: DateTime(2024),
      lastDate: DateTime(DateTime.now().year + 1),
    );

    if (picked == null) return;

    setState(() {
      if (isFrom) {
        _from = picked;
      } else {
        _to = picked;
      }
    });
  }

  static String _stamp(DateTime value) {
    String two(int v) => v.toString().padLeft(2, '0');
    return '${value.year}${two(value.month)}${two(value.day)}';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Business report', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'Generated on the server for your company, then viewed, saved or printed from here.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.lg),
        Expanded(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(width: 380, child: _buildControls(theme)),
              const SizedBox(width: AppSpacing.lg),
              Expanded(
                child: PdfReportView(
                  document: _document,
                  emptyMessage:
                      'Choose a period, then press GENERATE to build your report.',
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildControls(ThemeData theme) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Text('Choose a period', style: theme.textTheme.titleMedium),
          ),
          const Divider(height: 1),
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Container(
                    padding: const EdgeInsets.all(AppSpacing.sm + AppSpacing.xs),
                    decoration: BoxDecoration(
                      color: AppColors.sectionTint,
                      borderRadius:
                          BorderRadius.circular(AppSpacing.controlRadius),
                      border: Border.all(color: AppColors.border),
                    ),
                    child: Text(
                      'Rides completed and revenue for each of your drivers and vehicles, with the average rating passengers gave you.',
                      style: theme.textTheme.bodySmall,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.lg),
                  _DateRow(
                    label: 'From',
                    value: Formats.day(_from),
                    onTap: () => _pickDate(isFrom: true),
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  _DateRow(
                    label: 'To',
                    value: Formats.day(_to),
                    onTap: () => _pickDate(isFrom: false),
                  ),
                ],
              ),
            ),
          ),
          const Divider(height: 1),
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                FilledButton.icon(
                  onPressed: _isGenerating ? null : _generate,
                  icon: _isGenerating
                      ? const SizedBox(
                          height: 16,
                          width: 16,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Icon(Icons.picture_as_pdf_outlined, size: 18),
                  label: const Text('GENERATE'),
                ),
                const SizedBox(height: AppSpacing.sm),
                PdfReportActions(document: _document, fileName: _fileName),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _DateRow extends StatelessWidget {
  const _DateRow({
    required this.label,
    required this.value,
    required this.onTap,
  });

  final String label;
  final String value;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        SizedBox(
          width: 56,
          child: Text(label, style: theme.textTheme.bodyMedium),
        ),
        Expanded(
          child: InkWell(
            onTap: onTap,
            borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
            child: InputDecorator(
              decoration: const InputDecoration(),
              child: Row(
                children: [
                  const Icon(Icons.calendar_today_outlined,
                      size: 16, color: AppColors.textMuted),
                  const SizedBox(width: AppSpacing.sm),
                  Text(value, style: theme.textTheme.bodyMedium),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }
}

DateTime _defaultReportStart() {
  final now = DateTime.now();
  return DateTime(now.year, now.month - 5, 1);
}
