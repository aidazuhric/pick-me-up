import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../providers/order_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import 'driver_ride_detail_screen.dart';

class CompletedRidesScreen extends StatefulWidget {
  const CompletedRidesScreen({super.key});

  @override
  State<CompletedRidesScreen> createState() => _CompletedRidesScreenState();
}

class _CompletedRidesScreenState extends State<CompletedRidesScreen> {
  static const int _pageSize = 10;

  final _orders = OrderProvider();

  List<Order> _items = const [];
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
          'Status': 'Completed',
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'CompletedAt desc',
          'IncludeTotalCount': true,
        },
      );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Order>[];
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
            'You have not completed a ride yet.',
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        itemCount: _items.length,
        itemBuilder: (context, index) {
          final ride = _items[index];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _RideTile(
              ride: ride,
              onTap: ride.id == null
                  ? null
                  : () => DriverRideDetailScreen.open(context, ride.id!),
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
  const _RideTile({required this.ride, this.onTap});

  final Order ride;
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
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      ride.passengerName ?? '',
                      style: theme.textTheme.titleMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  Text(
                    Formats.money(ride.fare),
                    style: theme.textTheme.titleMedium,
                  ),
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
                      Formats.moment(ride.completedAt),
                      style: theme.textTheme.bodySmall,
                    ),
                  ),
                  if ((ride.carPlate ?? '').isNotEmpty)
                    Text(
                      ride.carPlate!,
                      style: theme.textTheme.bodySmall,
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
