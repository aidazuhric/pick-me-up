import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/support_ticket.dart';
import '../../providers/support_ticket_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';

class SupportTicketDetail extends StatefulWidget {
  const SupportTicketDetail({
    super.key,
    required this.ticket,
    required this.provider,
  });

  final SupportTicket ticket;
  final SupportTicketProvider provider;

  static Future<bool> show(
    BuildContext context, {
    required SupportTicket ticket,
    required SupportTicketProvider provider,
  }) async {
    final changed = await showDialog<bool>(
      context: context,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 820,
            height: 640,
            child: SupportTicketDetail(ticket: ticket, provider: provider),
          ),
        ),
      ),
    );

    return changed ?? false;
  }

  @override
  State<SupportTicketDetail> createState() => _SupportTicketDetailState();
}

class _SupportTicketDetailState extends State<SupportTicketDetail> {
  static const List<String> _statuses = ['Open', 'InProgress', 'Resolved'];

  final _replyController = TextEditingController();

  late SupportTicket _ticket = widget.ticket;
  bool _isWorking = false;
  bool _hasChanged = false;

  @override
  void dispose() {
    _replyController.dispose();
    super.dispose();
  }

  Future<void> _sendReply() async {
    final body = _replyController.text.trim();

    if (body.isEmpty) {
      AppMessages.failure(context, 'Write a reply before sending it.');
      return;
    }

    setState(() => _isWorking = true);

    try {
      final updated = await widget.provider.reply(_ticket.id!, body);
      if (!mounted) return;

      setState(() {
        _ticket = updated;
        _hasChanged = true;
        _isWorking = false;
      });

      _replyController.clear();
      AppMessages.success(context, 'Your reply was sent to ${updated.submitterName}.');
    } catch (e) {
      if (!mounted) return;
      setState(() => _isWorking = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  Future<void> _changeStatus(String status) async {
    setState(() => _isWorking = true);

    try {
      final updated = await widget.provider.changeStatus(_ticket.id!, status);
      if (!mounted) return;

      setState(() {
        _ticket = updated;
        _hasChanged = true;
        _isWorking = false;
      });

      AppMessages.success(context, 'The ticket is now ${_label(status)}.');
    } catch (e) {
      if (!mounted) return;
      setState(() => _isWorking = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  static String _label(String status) =>
      status == 'InProgress' ? 'In progress' : status;

  static String _moment(DateTime? value) {
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
    final replies = _ticket.replies ?? const <SupportTicketReply>[];

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
                  Text(_ticket.subject ?? '', style: theme.textTheme.headlineSmall),
                  const SizedBox(height: 2),
                  Text(
                    'Filed by ${_ticket.submitterName} on ${_moment(_ticket.createdAt)}',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
            _StatusPill(status: _ticket.status ?? ''),
            const SizedBox(width: AppSpacing.sm),
            IconButton(
              tooltip: 'Close',
              onPressed: () => Navigator.of(context).pop(_hasChanged),
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
                if (_ticket.targetUserName != null) ...[
                  _TargetBanner(name: _ticket.targetUserName!),
                  const SizedBox(height: AppSpacing.md),
                ],
                _Message(
                  author: _ticket.submitterName ?? '',
                  moment: _moment(_ticket.createdAt),
                  body: _ticket.body ?? '',
                  isAdmin: false,
                ),
                if (replies.isEmpty) ...[
                  const SizedBox(height: AppSpacing.md),
                  Text(
                    'No replies yet.',
                    style: theme.textTheme.bodySmall,
                  ),
                ],
                for (final reply in replies) ...[
                  const SizedBox(height: AppSpacing.sm),
                  _Message(
                    author: reply.authorName ?? '',
                    moment: _moment(reply.createdAt),
                    body: reply.body ?? '',
                    isAdmin: true,
                  ),
                ],
                if (_ticket.resolvedByName != null) ...[
                  const SizedBox(height: AppSpacing.md),
                  Text(
                    'Resolved by ${_ticket.resolvedByName} on ${_moment(_ticket.resolvedAt)}.',
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
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextField(
                controller: _replyController,
                enabled: !_isWorking,
                minLines: 2,
                maxLines: 4,
                style: theme.textTheme.bodyMedium,
                decoration: const InputDecoration(
                  hintText: 'Write a reply to the person who filed this ticket',
                ),
              ),
              const SizedBox(height: AppSpacing.md),
              Row(
                children: [
                  Text('Move to', style: theme.textTheme.bodySmall),
                  const SizedBox(width: AppSpacing.sm),
                  for (final status in _statuses) ...[
                    OutlinedButton(
                      onPressed: _isWorking || _ticket.status == status
                          ? null
                          : () => _changeStatus(status),
                      child: Text(_label(status)),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                  ],
                  const Spacer(),
                  FilledButton.icon(
                    onPressed: _isWorking ? null : _sendReply,
                    icon: const Icon(Icons.send_outlined, size: 16),
                    label: const Text('SEND REPLY'),
                  ),
                ],
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
        status == 'InProgress' ? 'In progress' : status,
        style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: color),
      ),
    );
  }
}

class _TargetBanner extends StatelessWidget {
  const _TargetBanner({required this.name});

  final String name;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.sm + AppSpacing.xs),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          const Icon(Icons.person_search_outlined, size: 18, color: AppColors.textMuted),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              'This ticket is about $name. Open Users to edit or suspend that account.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        ],
      ),
    );
  }
}

class _Message extends StatelessWidget {
  const _Message({
    required this.author,
    required this.moment,
    required this.body,
    required this.isAdmin,
  });

  final String author;
  final String moment;
  final String body;
  final bool isAdmin;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: isAdmin ? AppColors.sectionTint : AppColors.surface,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
        border: Border.all(
          color: isAdmin ? AppColors.border : AppColors.divider,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                isAdmin ? Icons.support_agent : Icons.person_outline,
                size: 16,
                color: isAdmin ? AppColors.primaryIndigo : AppColors.textMuted,
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
