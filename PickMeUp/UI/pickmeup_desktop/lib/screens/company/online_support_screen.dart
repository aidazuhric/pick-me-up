import 'package:flutter/material.dart';

import '../../models/search_result.dart';
import '../../models/support_ticket.dart';
import '../../providers/support_ticket_provider.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'support_ticket_form.dart';
import 'support_ticket_thread.dart';

class OnlineSupportScreen extends StatefulWidget {
  const OnlineSupportScreen({super.key});

  @override
  State<OnlineSupportScreen> createState() => _OnlineSupportScreenState();
}

class _OnlineSupportScreenState extends State<OnlineSupportScreen> {
  final _provider = SupportTicketProvider();
  final _tableKey = GlobalKey<CrudDataTableState<SupportTicket>>();

  String? _status;

  Future<SearchResult<SupportTicket>> _load(
    int page,
    int pageSize,
    String? search,
    String? sortBy,
  ) {
    return _provider.get(filter: {
      'Page': page,
      'PageSize': pageSize,
      'IncludeTotalCount': true,
      'SortBy': sortBy ?? 'CreatedAt desc',
      'Subject': ?search,
      'Status': ?_status,
    });
  }

  Future<bool> _submit() => SupportTicketForm.show(context, provider: _provider);

  Future<bool> _open(SupportTicket ticket) async {
    final full = await _provider.getById(ticket.id!);

    if (!mounted) return false;

    await SupportTicketThread.show(context, full);
    return false;
  }

  static String _day(DateTime? value) {
    if (value == null) {
      return '';
    }

    final local = value.toLocal();
    String two(int v) => v.toString().padLeft(2, '0');

    return '${local.year}-${two(local.month)}-${two(local.day)}';
  }

  static String _answer(SupportTicket ticket) {
    final replies = ticket.replies?.length ?? 0;

    if (replies == 0) {
      return 'Waiting for support';
    }

    return replies == 1 ? '1 reply' : '$replies replies';
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<SupportTicket>(
      key: _tableKey,
      title: 'Online support',
      subtitle: 'Ask for help and read what support answered.',
      searchHint: 'Search by subject',
      createLabel: 'ASK FOR HELP',
      loadPage: _load,
      onCreate: _submit,
      onEdit: _open,
      editTooltip: 'Read the conversation',
      editIcon: Icons.forum_outlined,
      filters: [
        FilterDropdown<String>(
          value: _status,
          width: 170,
          options: const [
            FilterOption<String>(label: 'Any status', value: null),
            FilterOption<String>(label: 'Open', value: 'Open'),
            FilterOption<String>(label: 'In progress', value: 'InProgress'),
            FilterOption<String>(label: 'Resolved', value: 'Resolved'),
          ],
          onChanged: (value) {
            setState(() => _status = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
      ],
      columns: [
        CrudColumn(
          label: 'Subject',
          value: (ticket) => ticket.subject ?? '',
          sortKey: 'Subject',
          maxWidth: 320,
        ),
        CrudColumn(
          label: 'Status',
          value: (ticket) => SupportTicketThread.statusLabel(ticket.status),
        ),
        CrudColumn(label: 'Answer', value: _answer, maxWidth: 180),
        CrudColumn(
          label: 'Sent on',
          value: (ticket) => _day(ticket.createdAt),
          sortKey: 'CreatedAt',
        ),
      ],
    );
  }
}
