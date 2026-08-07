import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/support_ticket.dart';

class SupportTicketThread extends StatelessWidget {
  const SupportTicketThread({super.key, required this.ticket});

  final SupportTicket ticket;

  static Future<void> show(BuildContext context, SupportTicket ticket) {
    return showDialog<void>(
      context: context,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 820,
            height: 600,
            child: SupportTicketThread(ticket: ticket),
          ),
        ),
      ),
    );
  }

  static String statusLabel(String? status) =>
      status == 'InProgress' ? 'In progress' : status ?? '';

  static String moment(DateTime? value) {
    if (value == null) {
      return '';
    }

    final local = value.toLocal();
    String two(int v) => v.toString().padLeft(2, '0');

    return '${local.year}-${two(local.month)}-${two(local.day)} '
        '${two(local.hour)}:${two(local.minute)}';
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final replies = ticket.replies ?? const <SupportTicketReply>[];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(ticket.subject ?? '',
                      style: theme.textTheme.headlineSmall),
                  const SizedBox(height: 2),
                  Text(
                    'You sent this on ${moment(ticket.createdAt)}',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
            _StatusPill(status: ticket.status ?? ''),
            const SizedBox(width: AppSpacing.sm),
            IconButton(
              tooltip: 'Close',
              onPressed: () => Navigator.of(context).pop(),
              icon: const Icon(Icons.close, size: 20),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        const Divider(height: 1),
        Expanded(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(vertical: AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _Message(
                  author: ticket.submitterName ?? '',
                  moment: moment(ticket.createdAt),
                  body: ticket.body ?? '',
                  isSupport: false,
                ),
                if (replies.isEmpty) ...[
                  const SizedBox(height: AppSpacing.md),
                  Text(
                    'Support has not replied yet. You will see the answer here.',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
                for (final reply in replies) ...[
                  const SizedBox(height: AppSpacing.sm),
                  _Message(
                    author: reply.authorName ?? '',
                    moment: moment(reply.createdAt),
                    body: reply.body ?? '',
                    isSupport: true,
                  ),
                ],
                if (ticket.resolvedByName != null) ...[
                  const SizedBox(height: AppSpacing.md),
                  Text(
                    'Closed by ${ticket.resolvedByName} on '
                    '${moment(ticket.resolvedAt)}.',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ],
            ),
          ),
        ),
        const Divider(height: 1),
        Padding(
          padding: const EdgeInsets.only(top: AppSpacing.md),
          child: Row(
            children: [
              const Icon(Icons.info_outline,
                  size: 16, color: AppColors.textMuted),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  'Send another ticket if you need to add something.',
                  style: theme.textTheme.bodySmall,
                ),
              ),
              OutlinedButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('BACK'),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final color = switch (status) {
      'Resolved' => AppColors.toggleOn,
      'InProgress' => AppColors.accentCyan,
      _ => AppColors.textMuted,
    };

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm + AppSpacing.xs,
        vertical: AppSpacing.xs + 2,
      ),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Text(
        SupportTicketThread.statusLabel(status),
        style:
            TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: color),
      ),
    );
  }
}

class _Message extends StatelessWidget {
  const _Message({
    required this.author,
    required this.moment,
    required this.body,
    required this.isSupport,
  });

  final String author;
  final String moment;
  final String body;
  final bool isSupport;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: isSupport ? AppColors.sectionTint : AppColors.surface,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(
          color: isSupport ? AppColors.border : AppColors.divider,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                isSupport ? Icons.support_agent : Icons.person_outline,
                size: 16,
                color: isSupport ? AppColors.primaryIndigo : AppColors.textMuted,
              ),
              const SizedBox(width: AppSpacing.sm),
              Text(author, style: theme.textTheme.titleSmall),
              const SizedBox(width: AppSpacing.sm),
              Text(moment, style: theme.textTheme.bodySmall),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(body, style: theme.textTheme.bodyMedium),
        ],
      ),
    );
  }
}
