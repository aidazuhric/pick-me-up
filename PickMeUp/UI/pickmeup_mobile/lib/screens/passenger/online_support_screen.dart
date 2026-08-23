import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/support_ticket.dart';
import '../../providers/support_ticket_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/status_pill.dart';
import 'support_ticket_form_screen.dart';
import 'support_ticket_thread_screen.dart';

class OnlineSupportScreen extends StatefulWidget {
  const OnlineSupportScreen({super.key});

  @override
  State<OnlineSupportScreen> createState() => _OnlineSupportScreenState();
}

class _OnlineSupportScreenState extends State<OnlineSupportScreen> {
  static const int _pageSize = 10;
  static const List<String> _statuses = [
    'All',
    'Open',
    'InProgress',
    'Resolved',
  ];

  final _tickets = SupportTicketProvider();

  List<SupportTicket> _items = const [];
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
      final result = await _tickets.get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'CreatedAt desc',
          'IncludeTotalCount': true,
          if (_status != 'All') 'Status': _status,
        },
      );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <SupportTicket>[];
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

  Future<void> _newTicket() async {
    final submitted = await SupportTicketFormScreen.open(context);

    if (submitted != true || !mounted) {
      return;
    }

    AppMessages.success(
      context,
      'Your ticket has been sent. Support will answer here.',
    );

    setState(() {
      _status = 'All';
      _page = 1;
    });

    await _load();
  }

  int get _pageCount =>
      _totalCount == 0 ? 1 : ((_totalCount + _pageSize - 1) ~/ _pageSize);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
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
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _newTicket,
        backgroundColor: AppColors.mobileHeader,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.support_agent),
        label: const Text('ASK FOR HELP'),
      ),
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
                ? 'You have not asked for help yet. Tap Ask for help to send a ticket.'
                : 'No ${Formats.statusLabel(_status).toLowerCase()} tickets.',
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
          88,
        ),
        itemCount: _items.length,
        itemBuilder: (context, index) {
          final ticket = _items[index];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _TicketTile(
              ticket: ticket,
              onTap: ticket.id == null
                  ? null
                  : () async {
                      await SupportTicketThreadScreen.open(context, ticket.id!);
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

class _TicketTile extends StatelessWidget {
  const _TicketTile({required this.ticket, this.onTap});

  final SupportTicket ticket;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final replyCount = ticket.replies?.length ?? 0;

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
                      ticket.subject ?? '',
                      style: theme.textTheme.titleMedium,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  StatusPill(status: ticket.status),
                ],
              ),
              const SizedBox(height: AppSpacing.xs),
              Text(
                ticket.body ?? '',
                style: theme.textTheme.bodySmall,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              const SizedBox(height: AppSpacing.xs),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      Formats.moment(ticket.createdAt),
                      style: theme.textTheme.bodySmall,
                    ),
                  ),
                  Icon(
                    replyCount == 0
                        ? Icons.mark_email_unread_outlined
                        : Icons.forum_outlined,
                    size: 14,
                    color: AppColors.textMuted,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    replyCount == 0
                        ? 'Awaiting reply'
                        : (replyCount == 1 ? '1 reply' : '$replyCount replies'),
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
