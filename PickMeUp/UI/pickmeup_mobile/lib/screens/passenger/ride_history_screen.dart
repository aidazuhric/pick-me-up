import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/taxi_company.dart';
import '../../providers/order_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/api_image.dart';
import '../../widgets/status_pill.dart';
import 'ride_detail_screen.dart';

class RideHistoryScreen extends StatefulWidget {
  const RideHistoryScreen({super.key});

  @override
  State<RideHistoryScreen> createState() => _RideHistoryScreenState();
}

class _RideHistoryScreenState extends State<RideHistoryScreen> {
  static const int _pageSize = 10;
  static const List<String> _statuses = [
    'All',
    'Completed',
    'Cancelled',
    'Pending',
    'Confirmed',
    'InProgress',
  ];

  final _orders = OrderProvider();
  final _companies = TaxiCompanyProvider();

  List<Order> _items = const [];
  Map<int, TaxiCompany> _companyById = const {};
  String _status = 'All';
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
      final result = await _orders.get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'CreatedAt desc',
          'IncludeTotalCount': true,
          if (_status != 'All') 'Status': _status,
        },
      );

      final companies = _companyById.isNotEmpty
          ? null
          : await _companies.get(
              filter: {'IsActive': true, 'Page': 1, 'PageSize': 100},
            );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Order>[];
        _totalCount = result.totalCount ?? 0;
        if (companies != null) {
          _companyById = {
            for (final c in companies.items ?? const <TaxiCompany>[])
              if (c.id != null) c.id!: c,
          };
        }
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
        SizedBox(
          height: 56,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.screenPadding,
              vertical: AppSpacing.sm,
            ),
            itemCount: _statuses.length,
            separatorBuilder: (_, _) => const SizedBox(width: AppSpacing.sm),
            itemBuilder: (context, index) {
              final value = _statuses[index];

              return ChoiceChip(
                label: Text(Formats.statusLabel(value)),
                selected: _status == value,
                onSelected: (_) {
                  setState(() {
                    _status = value;
                    _page = 1;
                  });
                  _load();
                },
              );
            },
          ),
        ),
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
            _status == 'All'
                ? 'You have not ordered a ride yet.'
                : 'No ${Formats.statusLabel(_status).toLowerCase()} rides.',
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(
          AppSpacing.screenPadding,
          0,
          AppSpacing.screenPadding,
          AppSpacing.md,
        ),
        itemCount: _items.length,
        itemBuilder: (context, index) {
          final ride = _items[index];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _RideTile(
              ride: ride,
              company: _companyById[ride.taxiCompanyId ?? -1],
              onTap: ride.id == null
                  ? null
                  : () async {
                      await RideDetailScreen.open(context, ride.id!);
                      if (mounted) {
                        await _load();
                      }
                    },
            ),
          );
        },
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

class _RideTile extends StatelessWidget {
  const _RideTile({required this.ride, required this.company, this.onTap});

  final Order ride;
  final TaxiCompany? company;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

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
          child: Row(
            children: [
              ApiImage.logo(url: company?.logoImageUrl, size: 46),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            ride.companyName ?? 'Any available driver',
                            style: theme.textTheme.titleMedium,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        StatusPill(status: ride.status),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      ride.destinationAddress == null
                          ? ride.pickupAddress ?? ''
                          : '${ride.pickupAddress ?? ''} → ${ride.destinationAddress}',
                      style: theme.textTheme.bodySmall,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            Formats.moment(ride.createdAt),
                            style: theme.textTheme.bodySmall,
                          ),
                        ),
                        if (ride.fare != null)
                          Text(
                            Formats.money(ride.fare),
                            style: theme.textTheme.titleMedium,
                          ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
