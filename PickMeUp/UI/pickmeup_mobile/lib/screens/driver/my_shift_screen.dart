import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/shift.dart';
import '../../providers/shift_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';

class MyShiftScreen extends StatefulWidget {
  const MyShiftScreen({super.key});

  @override
  State<MyShiftScreen> createState() => _MyShiftScreenState();
}

class _MyShiftScreenState extends State<MyShiftScreen> {
  final _shifts = ShiftProvider();

  Shift? _openShift;
  List<Shift> _recent = const [];
  bool _isLoading = true;
  bool _isBusy = false;
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
      final open = await _shifts.readOpenShift();
      final recent = await _shifts.get(
        filter: {'Page': 1, 'PageSize': 5, 'SortBy': 'StartedAt desc'},
      );

      if (!mounted) return;

      setState(() {
        _openShift = open;
        _recent = (recent.items ?? const <Shift>[])
            .where((s) => s.isOngoing != true)
            .toList();
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

  Future<void> _run(Future<Shift> Function() action, String success) async {
    setState(() => _isBusy = true);

    try {
      await action();

      if (!mounted) return;

      AppMessages.success(context, success);
      await _load();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  Future<void> _endShift(int shiftId) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('End shift'),
        content: const Text(
          'You will stop receiving ride requests. Any open break is closed too.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('CANCEL'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('END SHIFT'),
          ),
        ],
      ),
    );

    if (confirmed != true) {
      return;
    }

    await _run(() => _shifts.stop(shiftId), 'Shift ended.');
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

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          _buildStateCard(),
          const SizedBox(height: AppSpacing.md),
          _buildActions(),
          if (_openShift != null) ...[
            const SizedBox(height: AppSpacing.lg),
            _buildSummary(_openShift!),
          ],
          if (_recent.isNotEmpty) ...[
            const Divider(height: AppSpacing.xl),
            Text(
              'Earlier shifts',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: AppSpacing.sm),
            for (final shift in _recent)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                child: _PastShiftTile(shift: shift),
              ),
          ],
        ],
      ),
    );
  }

  Widget _buildStateCard() {
    final theme = Theme.of(context);
    final shift = _openShift;
    final onBreak = shift?.isOnBreak == true;
    final eligible = shift?.isDispatchEligible == true;

    final (colour, icon, title, reason) = switch ((shift, onBreak)) {
      (null, _) => (
        AppColors.textMuted,
        Icons.bedtime_outlined,
        'Off shift',
        'You will not receive ride requests until you start a shift.',
      ),
      (_, true) => (
        AppColors.toggleOff,
        Icons.free_breakfast_outlined,
        'On a break',
        'You will not receive ride requests while your break is running.',
      ),
      _ => (
        AppColors.toggleOn,
        Icons.check_circle,
        'On shift',
        'You are available — ride requests can reach you now.',
      ),
    };

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: colour.withValues(alpha: 0.1),
        border: Border.all(color: colour.withValues(alpha: 0.4)),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: colour, size: 26),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(title, style: theme.textTheme.headlineSmall),
              ),
              _EligibilityPill(eligible: eligible),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(reason, style: theme.textTheme.bodyMedium),
          if (shift?.startedAt != null) ...[
            const SizedBox(height: AppSpacing.sm),
            Text(
              'Started ${Formats.moment(shift!.startedAt)}',
              style: theme.textTheme.bodySmall,
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildActions() {
    final shift = _openShift;

    if (shift == null) {
      return FilledButton.icon(
        onPressed: _isBusy
            ? null
            : () => _run(_shifts.start, 'Shift started. You are available.'),
        icon: const Icon(Icons.play_arrow, size: 20),
        label: const Text('START SHIFT'),
      );
    }

    final shiftId = shift.id!;
    final onBreak = shift.isOnBreak == true;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (onBreak)
          FilledButton.icon(
            onPressed: _isBusy
                ? null
                : () => _run(
                    () => _shifts.stopBreak(shiftId),
                    'Break over. You are available again.',
                  ),
            icon: const Icon(Icons.play_arrow, size: 20),
            label: const Text('END BREAK'),
          )
        else
          FilledButton.icon(
            onPressed: _isBusy
                ? null
                : () => _run(
                    () => _shifts.startBreak(shiftId),
                    'Break started. You will not receive requests.',
                  ),
            icon: const Icon(Icons.free_breakfast_outlined, size: 20),
            label: const Text('TAKE A BREAK'),
          ),
        const SizedBox(height: AppSpacing.sm),
        OutlinedButton.icon(
          onPressed: _isBusy ? null : () => _endShift(shiftId),
          icon: const Icon(Icons.stop_circle_outlined, size: 20),
          label: const Text('END SHIFT'),
        ),
      ],
    );
  }

  Widget _buildSummary(Shift shift) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('This shift so far', style: theme.textTheme.titleMedium),
        const SizedBox(height: AppSpacing.sm),
        Row(
          children: [
            Expanded(
              child: _Stat(
                icon: Icons.timer_outlined,
                label: 'Worked',
                value: Formats.minutes(shift.workedMinutes),
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: _Stat(
                icon: Icons.free_breakfast_outlined,
                label: 'On break',
                value: Formats.minutes(shift.breakMinutes),
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: _Stat(
                icon: Icons.local_taxi_outlined,
                label: 'Rides',
                value: '${shift.ordersHandled ?? 0}',
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class _EligibilityPill extends StatelessWidget {
  const _EligibilityPill({required this.eligible});

  final bool eligible;

  @override
  Widget build(BuildContext context) {
    final colour = eligible ? AppColors.toggleOn : AppColors.textMuted;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: colour.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            eligible ? Icons.wifi_tethering : Icons.wifi_tethering_off,
            size: 14,
            color: colour,
          ),
          const SizedBox(width: 5),
          Text(
            eligible ? 'Available' : 'Not available',
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: colour,
            ),
          ),
        ],
      ),
    );
  }
}

class _Stat extends StatelessWidget {
  const _Stat({required this.icon, required this.label, required this.value});

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.md,
      ),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Column(
        children: [
          Icon(icon, size: 18, color: AppColors.mobileHeader),
          const SizedBox(height: AppSpacing.xs),
          FittedBox(
            child: Text(
              value,
              style: theme.textTheme.titleMedium,
              maxLines: 1,
            ),
          ),
          const SizedBox(height: 2),
          Text(label, style: theme.textTheme.bodySmall),
        ],
      ),
    );
  }
}

class _PastShiftTile extends StatelessWidget {
  const _PastShiftTile({required this.shift});

  final Shift shift;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  Formats.day(shift.startedAt),
                  style: theme.textTheme.titleMedium,
                ),
                Text(
                  '${Formats.minutes(shift.workedMinutes)} worked · '
                  '${Formats.minutes(shift.breakMinutes)} break',
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
          ),
          Text(
            '${shift.ordersHandled ?? 0} '
            '${(shift.ordersHandled ?? 0) == 1 ? 'ride' : 'rides'}',
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}
