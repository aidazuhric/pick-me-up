import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/payment.dart';
import '../../models/search_result.dart';
import '../../providers/payment_provider.dart';
import '../../utils/formats.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';

class PaymentOverviewScreen extends StatefulWidget {
  const PaymentOverviewScreen({super.key});

  @override
  State<PaymentOverviewScreen> createState() => _PaymentOverviewScreenState();
}

class _PaymentOverviewScreenState extends State<PaymentOverviewScreen> {
  final _provider = PaymentProvider();
  final _tableKey = GlobalKey<CrudDataTableState<Payment>>();

  String? _status;
  String? _period;

  Future<SearchResult<Payment>> _load(
    int page,
    int pageSize,
    String? search,
    String? sortBy,
  ) {
    return _provider.get(filter: {
      'Page': page,
      'PageSize': pageSize,
      'IncludeTotalCount': true,
      'SortBy': ?sortBy,
      'PassengerName': ?search,
      'Status': ?_status,
      'Period': ?_period,
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Payments', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'What passengers were charged for your rides.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.md),
        const _ReadOnlyNotice(),
        const SizedBox(height: AppSpacing.md),
        Expanded(
          child: CrudDataTable<Payment>(
            key: _tableKey,
            title: 'Payment history',
            searchHint: 'Search by passenger name',
            loadPage: _load,
            filters: [
              FilterDropdown<String>(
                value: _status,
                width: 190,
                options: const [
                  FilterOption<String>(label: 'Any status', value: null),
                  FilterOption<String>(label: 'Unpaid', value: 'Unpaid'),
                  FilterOption<String>(label: 'Paid', value: 'Paid'),
                  FilterOption<String>(
                    label: 'Partially refunded',
                    value: 'PartiallyRefunded',
                  ),
                  FilterOption<String>(label: 'Refunded', value: 'Refunded'),
                ],
                onChanged: (value) {
                  setState(() => _status = value);
                  _tableKey.currentState?.refresh(toFirstPage: true);
                },
              ),
              FilterDropdown<String>(
                value: _period,
                width: 160,
                options: const [
                  FilterOption<String>(label: 'All time', value: null),
                  FilterOption<String>(label: 'Last day', value: 'Day'),
                  FilterOption<String>(label: 'Last week', value: 'Week'),
                  FilterOption<String>(label: 'Last month', value: 'Month'),
                ],
                onChanged: (value) {
                  setState(() => _period = value);
                  _tableKey.currentState?.refresh(toFirstPage: true);
                },
              ),
            ],
            columns: [
              CrudColumn(
                label: 'Ride',
                value: (p) => 'Ride #${p.orderId}',
              ),
              CrudColumn(
                label: 'Passenger',
                value: (p) => p.passengerName ?? '',
                maxWidth: 160,
              ),
              CrudColumn(
                label: 'Driver',
                value: (p) => p.driverName ?? '',
                maxWidth: 160,
              ),
              CrudColumn(
                label: 'Charged',
                value: (p) => Formats.money(p.amountCharged, p.currency ?? 'BAM'),
              ),
              CrudColumn(
                label: 'Status',
                value: (p) => Formats.statusLabel(p.status),
              ),
              CrudColumn(
                label: 'Refunded',
                value: (p) => (p.refundedTotal ?? 0) == 0
                    ? '—'
                    : Formats.money(p.refundedTotal, p.currency ?? 'BAM'),
              ),
              CrudColumn(
                label: 'Paid on',
                value: (p) => p.paidAt == null ? '—' : Formats.day(p.paidAt),
                sortKey: 'PaidAt',
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _ReadOnlyNotice extends StatelessWidget {
  const _ReadOnlyNotice();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        children: [
          const Icon(Icons.lock_outline, size: 18, color: AppColors.textMuted),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              'Amounts and statuses are set by the payment provider when a charge is confirmed. They are shown here for reference and cannot be changed from this screen.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        ],
      ),
    );
  }
}
