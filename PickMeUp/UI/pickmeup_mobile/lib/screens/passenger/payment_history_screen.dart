import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/payment.dart';
import '../../providers/payment_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/status_pill.dart';
import 'ride_detail_screen.dart';

class PaymentHistoryScreen extends StatefulWidget {
  const PaymentHistoryScreen({super.key});

  @override
  State<PaymentHistoryScreen> createState() => _PaymentHistoryScreenState();
}

class _PaymentHistoryScreenState extends State<PaymentHistoryScreen> {
  static const int _pageSize = 10;

  final _payments = PaymentProvider();

  List<Payment> _items = const [];
  int _page = 1;
  int _totalCount = 0;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final result = await _payments.get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'CreatedAt desc',
          'IncludeTotalCount': true,
        },
      );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Payment>[];
        _totalCount = result.totalCount ?? 0;
        _isLoading = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  int get _pageCount =>
      _totalCount == 0 ? 1 : ((_totalCount + _pageSize - 1) ~/ _pageSize);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Expanded(child: _buildList()),
        if (!_isLoading && _error == null && _pageCount > 1) _buildPager(),
      ],
    );
  }

  Widget _buildList() {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    if (_items.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            'You have not paid for a ride yet.',
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          for (final payment in _items)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.sm),
              child: _PaymentTile(
                payment: payment,
                onTap: payment.orderId == null
                    ? null
                    : () => RideDetailScreen.open(context, payment.orderId!),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildPager() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.screenPadding,
        0,
        AppSpacing.screenPadding,
        AppSpacing.md,
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          IconButton(
            tooltip: 'Previous page',
            icon: const Icon(Icons.chevron_left),
            onPressed: _page > 1
                ? () {
                    setState(() => _page -= 1);
                    _load();
                  }
                : null,
          ),
          Text(
            'Page $_page of $_pageCount',
            style: Theme.of(context).textTheme.bodySmall,
          ),
          IconButton(
            tooltip: 'Next page',
            icon: const Icon(Icons.chevron_right),
            onPressed: _page < _pageCount
                ? () {
                    setState(() => _page += 1);
                    _load();
                  }
                : null,
          ),
        ],
      ),
    );
  }
}

class _PaymentTile extends StatelessWidget {
  const _PaymentTile({required this.payment, this.onTap});

  final Payment payment;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final currency = payment.currency ?? 'BAM';
    final refunded = payment.refundedTotal ?? 0;

    return Material(
      color: AppColors.surface,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            border: Border.all(color: AppColors.border),
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      payment.companyName ?? 'PickMeUp ride',
                      style: theme.textTheme.titleMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  StatusPill(status: payment.status),
                ],
              ),
              const SizedBox(height: AppSpacing.xs),
              Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Ride #${payment.orderId}',
                          style: theme.textTheme.bodySmall,
                        ),
                        Text(
                          Formats.moment(payment.paidAt ?? payment.createdAt),
                          style: theme.textTheme.bodySmall,
                        ),
                        if (payment.driverName != null)
                          Text(
                            payment.driverName!,
                            style: theme.textTheme.bodySmall,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                      ],
                    ),
                  ),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        Formats.money(payment.amountCharged, currency),
                        style: theme.textTheme.titleMedium,
                      ),
                      if (refunded > 0)
                        Text(
                          '−${Formats.money(refunded, currency)} refunded',
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: AppColors.accentCyan,
                          ),
                        ),
                    ],
                  ),
                  if (onTap != null) ...[
                    const SizedBox(width: AppSpacing.xs),
                    const Icon(Icons.chevron_right, color: AppColors.textMuted),
                  ],
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
