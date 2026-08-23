import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/support_ticket.dart';
import '../../providers/support_ticket_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/status_pill.dart';

class SupportTicketThreadScreen extends StatefulWidget {
  const SupportTicketThreadScreen({super.key, required this.ticketId});

  final int ticketId;

  static Future<void> open(BuildContext context, int ticketId) {
    return Navigator.of(context).push<void>(
      MaterialPageRoute<void>(
        builder: (_) => SupportTicketThreadScreen(ticketId: ticketId),
      ),
    );
  }

  @override
  State<SupportTicketThreadScreen> createState() =>
      _SupportTicketThreadScreenState();
}

class _SupportTicketThreadScreenState extends State<SupportTicketThreadScreen> {
  final _tickets = SupportTicketProvider();

  SupportTicket? _ticket;
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
      final ticket = await _tickets.getById(widget.ticketId);

      if (!mounted) return;

      setState(() {
        _ticket = ticket;
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Your ticket')),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    final ticket = _ticket;

    if (_error != null || ticket == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error ?? 'This ticket could not be loaded.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final theme = Theme.of(context);
    final replies = ticket.replies ?? const <SupportTicketReply>[];

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.screenPadding),
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Text(
                ticket.subject ?? '',
                style: theme.textTheme.headlineSmall,
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            StatusPill(status: ticket.status),
          ],
        ),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'You sent this on ${Formats.moment(ticket.createdAt)}',
          style: theme.textTheme.bodySmall,
        ),
        if (ticket.targetUserName != null)
          Text(
            'About ${ticket.targetUserName}',
            style: theme.textTheme.bodySmall,
          ),
        const SizedBox(height: AppSpacing.lg),
        _Message(
          author: ticket.submitterName ?? '',
          moment: Formats.moment(ticket.createdAt),
          body: ticket.body ?? '',
          isSupport: false,
        ),
        if (replies.isEmpty)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.md),
            child: Text(
              'Support has not replied yet. You will see the answer here.',
              style: theme.textTheme.bodySmall,
            ),
          ),
        for (final reply in replies)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.sm),
            child: _Message(
              author: reply.authorName ?? '',
              moment: Formats.moment(reply.createdAt),
              body: reply.body ?? '',
              isSupport: true,
            ),
          ),
        if (ticket.resolvedByName != null)
          Padding(
            padding: const EdgeInsets.only(top: AppSpacing.md),
            child: Text(
              'Closed by ${ticket.resolvedByName} on '
              '${Formats.moment(ticket.resolvedAt)}.',
              style: theme.textTheme.bodySmall,
            ),
          ),
        const SizedBox(height: AppSpacing.lg),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Icon(
              Icons.info_outline,
              size: 16,
              color: AppColors.textMuted,
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Text(
                'Send another ticket if you need to add something.',
                style: theme.textTheme.bodySmall,
              ),
            ),
          ],
        ),
      ],
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
        color: isSupport
            ? AppColors.accentCyan.withValues(alpha: 0.1)
            : AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                isSupport ? Icons.support_agent : Icons.person_outline,
                size: 16,
                color: isSupport
                    ? AppColors.accentCyan
                    : AppColors.mobileHeader,
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  author,
                  style: theme.textTheme.titleSmall,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
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
