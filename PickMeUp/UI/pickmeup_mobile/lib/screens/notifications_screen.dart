import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../models/notification_item.dart';
import '../providers/notification_store.dart';
import '../screens/passenger/ride_detail_screen.dart';
import '../utils/formats.dart';
import '../widgets/app_messages.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  bool? _isReadFilter;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<NotificationStore>().load(isRead: _isReadFilter);
    });
  }

  Future<void> _reload() {
    return context.read<NotificationStore>().load(isRead: _isReadFilter);
  }

  Future<void> _markAllRead() async {
    final store = context.read<NotificationStore>();
    final ok = await store.markAllRead();

    if (!mounted) return;

    if (ok) {
      AppMessages.success(context, 'All notifications marked as read.');
      await _reload();
    } else {
      AppMessages.failure(context, 'Could not mark them read. Try again.');
    }
  }

  Future<void> _open(NotificationItem item) async {
    final store = context.read<NotificationStore>();
    await store.markRead(item);

    final orderId = item.orderId;

    if (orderId == null || !mounted) {
      return;
    }

    await RideDetailScreen.open(context, orderId);
  }

  @override
  Widget build(BuildContext context) {
    final store = context.watch<NotificationStore>();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          if (store.unreadCount > 0)
            TextButton(
              onPressed: _markAllRead,
              child: const Text(
                'MARK ALL READ',
                style: TextStyle(color: Colors.white, fontSize: 12),
              ),
            ),
        ],
      ),
      body: Column(
        children: [
          _buildFilters(store),
          Expanded(child: _buildList(store)),
        ],
      ),
    );
  }

  Widget _buildFilters(NotificationStore store) {
    const options = <String, bool?>{
      'All': null,
      'Unread': false,
      'Read': true,
    };

    return SizedBox(
      height: 56,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.screenPadding,
          vertical: AppSpacing.sm,
        ),
        children: [
          for (final entry in options.entries) ...[
            ChoiceChip(
              label: Text(
                entry.key == 'Unread' && store.unreadCount > 0
                    ? 'Unread (${store.unreadCount})'
                    : entry.key,
              ),
              selected: _isReadFilter == entry.value,
              onSelected: (_) {
                setState(() => _isReadFilter = entry.value);
                _reload();
              },
            ),
            const SizedBox(width: AppSpacing.sm),
          ],
        ],
      ),
    );
  }

  Widget _buildList(NotificationStore store) {
    if (store.isLoading && store.items.isEmpty) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (store.error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            store.error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    if (store.items.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 84,
                height: 84,
                decoration: const BoxDecoration(
                  color: AppColors.sectionTint,
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.notifications_none,
                  size: 38,
                  color: AppColors.mobileHeader,
                ),
              ),
              const SizedBox(height: AppSpacing.lg),
              Text(
                _isReadFilter == false
                    ? 'Nothing unread.'
                    : 'No notifications yet.',
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                'Updates about your rides and payments arrive here.',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ],
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _reload,
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(
          AppSpacing.screenPadding,
          0,
          AppSpacing.screenPadding,
          AppSpacing.md,
        ),
        itemCount: store.items.length,
        itemBuilder: (context, index) {
          final item = store.items[index];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _NotificationTile(item: item, onTap: () => _open(item)),
          );
        },
      ),
    );
  }
}

class _NotificationTile extends StatelessWidget {
  const _NotificationTile({required this.item, required this.onTap});

  final NotificationItem item;
  final VoidCallback onTap;

  static IconData _iconFor(String? category) => switch (category) {
    'payment' => Icons.credit_card,
    'order' => Icons.local_taxi,
    'diagnostics' => Icons.bug_report_outlined,
    _ => Icons.notifications_none,
  };

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isUnread = item.isRead != true;

    return Material(
      color: isUnread
          ? AppColors.accentCyan.withValues(alpha: 0.07)
          : AppColors.surface,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            border: Border.all(
              color: isUnread ? AppColors.accentCyan : AppColors.border,
            ),
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Icon(
                _iconFor(item.category),
                size: 20,
                color: isUnread ? AppColors.accentCyan : AppColors.textMuted,
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            item.title ?? '',
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: isUnread
                                  ? FontWeight.w700
                                  : FontWeight.w500,
                            ),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        if (isUnread)
                          Container(
                            width: 9,
                            height: 9,
                            margin: const EdgeInsets.only(left: AppSpacing.sm),
                            decoration: const BoxDecoration(
                              color: AppColors.accentCyan,
                              shape: BoxShape.circle,
                            ),
                          ),
                      ],
                    ),
                    const SizedBox(height: 2),
                    Text(
                      item.body ?? '',
                      style: theme.textTheme.bodySmall,
                      maxLines: 3,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            Formats.moment(item.createdAt),
                            style: theme.textTheme.bodySmall,
                          ),
                        ),
                        if (item.orderId != null)
                          Text(
                            'Ride #${item.orderId}',
                            style: theme.textTheme.bodySmall,
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
