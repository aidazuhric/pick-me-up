import 'dart:typed_data';

import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../providers/report_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/pdf_report_view.dart';

enum _ReportKind { ordersByPeriod, topCompanies }

class PdfReportsScreen extends StatefulWidget {
  const PdfReportsScreen({super.key});

  @override
  State<PdfReportsScreen> createState() => _PdfReportsScreenState();
}

class _PdfReportsScreenState extends State<PdfReportsScreen> {
  final _provider = ReportProvider();

  _ReportKind _kind = _ReportKind.ordersByPeriod;
  String _granularity = 'day';
  late DateTime _from = _defaultReportStart();
  late DateTime _to = DateTime.now();

  Uint8List? _document;
  String? _fileName;
  bool _isGenerating = false;

  String get _title => _kind == _ReportKind.ordersByPeriod
      ? 'Orders by period and city'
      : 'Top 5 taxi companies';

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
      final bytes = _kind == _ReportKind.ordersByPeriod
          ? await _provider.ordersByPeriod(
              from: _from,
              to: _to,
              granularity: _granularity,
            )
          : await _provider.topCompanies(from: _from, to: _to);

      if (!mounted) return;

      setState(() {
        _document = bytes;
        _fileName = _suggestedName();
        _isGenerating = false;
      });

      AppMessages.success(context, '$_title is ready for ${_periodLabel()}.');
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

  String _suggestedName() {
    final slug = _kind == _ReportKind.ordersByPeriod
        ? 'orders-by-period'
        : 'top-companies';
    return '$slug-${_stamp(_from)}-${_stamp(_to)}.pdf';
  }

  static String _stamp(DateTime value) {
    String two(int v) => v.toString().padLeft(2, '0');
    return '${value.year}${two(value.month)}${two(value.day)}';
  }

  static String _day(DateTime value) {
    String two(int v) => v.toString().padLeft(2, '0');
    return '${value.year}-${two(value.month)}-${two(value.day)}';
  }

  String _periodLabel() => '${_day(_from)} to ${_day(_to)}';

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('PDF reports', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'Generated on the server, then viewed, saved or printed from here.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.lg),
        Expanded(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(width: 380, child: _buildControls(context)),
              const SizedBox(width: AppSpacing.lg),
              Expanded(
                child: PdfReportView(
                  document: _document,
                  emptyMessage:
                      'Choose a report and a period, then press GENERATE.',
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildControls(BuildContext context) {
    final theme = Theme.of(context);

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
            child: Text('Choose a report', style: theme.textTheme.titleMedium),
          ),
          const Divider(height: 1),
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _ReportChoice(
                    label: 'Orders by period and city',
                    description:
                        'Orders placed, cancellations and revenue, grouped by period and by the city of the company that served the ride.',
                    isSelected: _kind == _ReportKind.ordersByPeriod,
                    onSelected: () =>
                        setState(() => _kind = _ReportKind.ordersByPeriod),
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  _ReportChoice(
                    label: 'Top 5 taxi companies',
                    description:
                        'The five companies with the most completed rides, with revenue and average rating.',
                    isSelected: _kind == _ReportKind.topCompanies,
                    onSelected: () =>
                        setState(() => _kind = _ReportKind.topCompanies),
                  ),
                  const SizedBox(height: AppSpacing.lg),
                  Text('Period', style: theme.textTheme.titleSmall),
                  const SizedBox(height: AppSpacing.sm),
                  _DateRow(
                    label: 'From',
                    value: _day(_from),
                    onTap: () => _pickDate(isFrom: true),
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  _DateRow(
                    label: 'To',
                    value: _day(_to),
                    onTap: () => _pickDate(isFrom: false),
                  ),
                  if (_kind == _ReportKind.ordersByPeriod) ...[
                    const SizedBox(height: AppSpacing.lg),
                    Text('Group by', style: theme.textTheme.titleSmall),
                    const SizedBox(height: AppSpacing.sm),
                    SegmentedButton<String>(
                      showSelectedIcon: false,
                      style: SegmentedButton.styleFrom(
                        textStyle: theme.textTheme.bodyMedium,
                      ),
                      segments: const [
                        ButtonSegment(value: 'day', label: Text('Day')),
                        ButtonSegment(value: 'week', label: Text('Week')),
                        ButtonSegment(value: 'month', label: Text('Month')),
                      ],
                      selected: {_granularity},
                      onSelectionChanged: (selection) =>
                          setState(() => _granularity = selection.first),
                    ),
                  ],
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

class _ReportChoice extends StatelessWidget {
  const _ReportChoice({
    required this.label,
    required this.description,
    required this.isSelected,
    required this.onSelected,
  });

  final String label;
  final String description;
  final bool isSelected;
  final VoidCallback onSelected;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return InkWell(
      onTap: onSelected,
      borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.sm + AppSpacing.xs),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.rowHover : AppColors.surface,
          borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
          border: Border.all(
            color: isSelected ? AppColors.primaryIndigo : AppColors.border,
          ),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(
              isSelected ? Icons.radio_button_checked : Icons.radio_button_off,
              size: 18,
              color:
                  isSelected ? AppColors.primaryIndigo : AppColors.textMuted,
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(label, style: theme.textTheme.titleSmall),
                  const SizedBox(height: 2),
                  Text(description, style: theme.textTheme.bodySmall),
                ],
              ),
            ),
          ],
        ),
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
