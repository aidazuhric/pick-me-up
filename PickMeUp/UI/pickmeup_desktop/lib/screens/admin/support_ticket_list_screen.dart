import 'package:flutter/material.dart';

import '../../models/search_result.dart';
import '../../models/support_ticket.dart';
import '../../providers/support_ticket_provider.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'support_ticket_detail.dart';

class SupportTicketListScreen extends StatefulWidget {
  const SupportTicketListScreen({super.key});

  @override
  State<SupportTicketListScreen> createState() =>
      _SupportTicketListScreenState();
}

class _SupportTicketListScreenState extends State<SupportTicketListScreen> {
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
      'SortBy': ?sortBy,
      'Subject': ?search,
      'Status': ?_status,
    });
  }

  Future<bool> _open(SupportTicket ticket) {
    return SupportTicketDetail.show(
      context,
      ticket: ticket,
      provider: _provider,
    );
  }

  static String _moment(DateTime? value) {
    if (value == null) {
      return '';
    }

    final local = value.toLocal();
    String two(int v) => v.toString().padLeft(2, '0');

    return '${local.year}-${two(local.month)}-${two(local.day)}';
  }

  static String _statusLabel(String? status) =>
      status == 'InProgress' ? 'In progress' : status ?? '';

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<SupportTicket>(
      key: _tableKey,
      title: 'Reports & bugs',
      subtitle: 'Everything passengers, drivers and companies have reported.',
      searchHint: 'Search by subject',
      loadPage: _load,
      onEdit: _open,
      editTooltip: 'Open ticket',
      editIcon: Icons.open_in_new,
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
          maxWidth: 280,
        ),
        CrudColumn(
          label: 'Filed by',
          value: (ticket) => ticket.submitterName ?? '',
          maxWidth: 160,
        ),
        CrudColumn(
          label: 'About',
          value: (ticket) => ticket.targetUserName ?? '—',
          maxWidth: 160,
        ),
        CrudColumn(
          label: 'Status',
          value: (ticket) => _statusLabel(ticket.status),
        ),
        CrudColumn(
          label: 'Replies',
          value: (ticket) => (ticket.replies?.length ?? 0).toString(),
        ),
        CrudColumn(
          label: 'Filed on',
          value: (ticket) => _moment(ticket.createdAt),
          sortKey: 'CreatedAt',
        ),
      ],
    );
  }
}
