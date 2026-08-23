import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/shift.dart';
import '../../providers/notification_store.dart';
import '../../providers/order_provider.dart';
import '../../providers/shift_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';

class RideRequestsScreen extends StatefulWidget {
  const RideRequestsScreen({super.key});

  @override
  State<RideRequestsScreen> createState() => _RideRequestsScreenState();
}

class _RideRequestsScreenState extends State<RideRequestsScreen> {
  static const String _availableCategory = 'order-available';

  final _orders = OrderProvider();
  final _shifts = ShiftProvider();

  NotificationStore? _store;
  int? _lastSeenNotificationId;

  Shift? _openShift;
  List<Order> _requests = const [];
  Order? _justAccepted;
  bool _isLoading = true;
  int? _busyOrderId;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      final store = context.read<NotificationStore>();
      _store = store;
      _lastSeenNotificationId = store.items.isEmpty ? null : store.items.first.id;
      store.addListener(_onNotifications);
    });
  }

  @override
  void dispose() {
    _store?.removeListener(_onNotifications);
    super.dispose();
  }

  void _onNotifications() {
    final store = _store;

    if (store == null || store.items.isEmpty || !mounted) {
      return;
    }

    final newest = store.items.first;

    if (newest.id == _lastSeenNotificationId) {
      return;
    }

    _lastSeenNotificationId = newest.id;

    if (newest.category == _availableCategory) {
      _load();
    }
  }

  bool get _isEligible => _openShift?.isDispatchEligible == true;

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final shift = await _shifts.readOpenShift();

      var available = const <Order>[];

      if (shift?.isDispatchEligible == true) {
        final result = await _orders.get(
          filter: {
            'AvailableForMe': true,
            'Page': 1,
            'PageSize': 20,
            'SortBy': 'CreatedAt desc',
          },
        );
        available = result.items ?? const <Order>[];
      }

      if (!mounted) return;

      setState(() {
        _openShift = shift;
        _requests = available;
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

  Future<void> _accept(Order order) async {
    setState(() => _busyOrderId = order.id);

    try {
      final accepted = await _orders.accept(order.id!);

      if (!mounted) return;

      setState(() => _justAccepted = accepted);
      AppMessages.success(
        context,
        'Ride #${accepted.id} is yours. The passenger has been told.',
      );

      await _load();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      AppMessages.failure(context, e.message);
      await _load();
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _busyOrderId = null);
      }
    }
  }

  Future<void> _reject(Order order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Decline this ride?'),
        content: const Text(
          'It stays open for other drivers, but you will not see it again.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('KEEP IT'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('DECLINE'),
          ),
        ],
      ),
    );

    if (confirmed != true) {
      return;
    }

    setState(() => _busyOrderId = order.id);

    try {
      await _orders.reject(order.id!);

      if (!mounted) return;

      AppMessages.success(
        context,
        'Declined. Another driver can still take it.',
      );

      await _load();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      AppMessages.failure(context, e.message);
      await _load();
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _busyOrderId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
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

    if (!_isEligible) {
      return _buildIneligible();
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          if (_justAccepted != null) _buildAcceptedBanner(_justAccepted!),
          if (_requests.isEmpty)
            _buildEmpty()
          else
            for (final order in _requests)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                child: _RequestCard(
                  order: order,
                  isBusy: _busyOrderId == order.id,
                  onAccept: () => _accept(order),
                  onReject: () => _reject(order),
                ),
              ),
        ],
      ),
    );
  }

  Widget _buildIneligible() {
    final theme = Theme.of(context);
    final onBreak = _openShift?.isOnBreak == true;

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 84,
              height: 84,
              decoration: BoxDecoration(
                color: AppColors.sectionTint,
                shape: BoxShape.circle,
              ),
              child: Icon(
                onBreak ? Icons.free_breakfast_outlined : Icons.bedtime_outlined,
                size: 38,
                color: AppColors.mobileHeader,
              ),
            ),
            const SizedBox(height: AppSpacing.lg),
            Text(
              onBreak ? 'You are on a break' : 'You are off shift',
              style: theme.textTheme.headlineSmall,
            ),
            const SizedBox(height: AppSpacing.sm),
            Text(
              onBreak
                  ? 'Ride requests will not reach you until your break ends. '
                        'End the break in My shift.'
                  : 'Ride requests only reach drivers on shift. '
                        'Start a shift in My shift to see them.',
              textAlign: TextAlign.center,
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.lg),
            OutlinedButton.icon(
              onPressed: _load,
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text('CHECK AGAIN'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmpty() {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(top: AppSpacing.xl),
      child: Column(
        children: [
          Container(
            width: 84,
            height: 84,
            decoration: const BoxDecoration(
              color: AppColors.sectionTint,
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.hourglass_empty,
              size: 38,
              color: AppColors.mobileHeader,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          Text('No requests right now', style: theme.textTheme.headlineSmall),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'You are on shift and available. New rides will appear here.',
            textAlign: TextAlign.center,
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }

  Widget _buildAcceptedBanner(Order order) {
    final theme = Theme.of(context);

    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.toggleOn.withValues(alpha: 0.1),
        border: Border.all(color: AppColors.toggleOn.withValues(alpha: 0.4)),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.check_circle, color: AppColors.toggleOn, size: 20),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Ride #${order.id} is your active ride',
                  style: theme.textTheme.titleMedium,
                ),
                const SizedBox(height: 2),
                Text(
                  '${order.passengerName ?? ''} · '
                  '${order.carPlate ?? ''} · the passenger has been notified.',
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _RequestCard extends StatelessWidget {
  const _RequestCard({
    required this.order,
    required this.isBusy,
    required this.onAccept,
    required this.onReject,
  });

  final Order order;
  final bool isBusy;
  final VoidCallback onAccept;
  final VoidCallback onReject;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isGeneral = order.isGeneralOrder == true;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              const Icon(
                Icons.person_outline,
                size: 18,
                color: AppColors.textMuted,
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  order.passengerName ?? '',
                  style: theme.textTheme.titleMedium,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              _SourcePill(isGeneral: isGeneral, companyName: order.companyName),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          _Line(
            icon: Icons.my_location,
            colour: AppColors.toggleOn,
            text: order.pickupAddress ?? '',
          ),
          if ((order.destinationAddress ?? '').isNotEmpty)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.xs),
              child: _Line(
                icon: Icons.flag,
                colour: AppColors.danger,
                text: order.destinationAddress!,
              ),
            ),
          const SizedBox(height: AppSpacing.sm),
          Row(
            children: [
              Expanded(
                child: Text(
                  order.distanceKm == null
                      ? 'Distance not recorded'
                      : Formats.distance(order.distanceKm),
                  style: theme.textTheme.bodySmall,
                ),
              ),
              Text(
                Formats.moment(order.createdAt),
                style: theme.textTheme.bodySmall,
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: isBusy ? null : onReject,
                  child: const Text('DECLINE'),
                ),
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                flex: 2,
                child: FilledButton(
                  onPressed: isBusy ? null : onAccept,
                  child: isBusy
                      ? const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Text('ACCEPT'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _SourcePill extends StatelessWidget {
  const _SourcePill({required this.isGeneral, required this.companyName});

  final bool isGeneral;
  final String? companyName;

  @override
  Widget build(BuildContext context) {
    final colour = isGeneral ? AppColors.accentCyan : AppColors.mobileHeader;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: colour.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Text(
        isGeneral ? 'Open to all' : (companyName ?? 'Your company'),
        style: TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: colour,
        ),
      ),
    );
  }
}

class _Line extends StatelessWidget {
  const _Line({required this.icon, required this.colour, required this.text});

  final IconData icon;
  final Color colour;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 15, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(
            text,
            style: Theme.of(context).textTheme.bodySmall,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}
