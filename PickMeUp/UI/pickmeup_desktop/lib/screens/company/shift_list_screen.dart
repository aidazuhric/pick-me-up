import 'package:flutter/material.dart';

import '../../models/search_result.dart';
import '../../models/shift.dart';
import '../../providers/shift_provider.dart';
import '../../utils/formats.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';

class ShiftListScreen extends StatefulWidget {
  const ShiftListScreen({super.key});

  @override
  State<ShiftListScreen> createState() => _ShiftListScreenState();
}

class _ShiftListScreenState extends State<ShiftListScreen> {
  final _provider = ShiftProvider();
  final _tableKey = GlobalKey<CrudDataTableState<Shift>>();

  String? _period;
  bool? _ongoingOnly;

  Future<SearchResult<Shift>> _load(
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
      'DriverName': ?search,
      'Period': ?_period,
      'OngoingOnly': ?_ongoingOnly,
    });
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<Shift>(
      key: _tableKey,
      title: 'Driver shifts',
      subtitle: 'When your drivers worked, and how much they handled.',
      searchHint: 'Search by driver name',
      loadPage: _load,
      filters: [
        FilterDropdown<String>(
          value: _period,
          width: 160,
          options: const [
            FilterOption<String>(label: 'All time', value: null),
            FilterOption<String>(label: 'Last day', value: 'Day'),
            FilterOption<String>(label: 'Last week', value: 'Week'),
            FilterOption<String>(label: 'Last month', value: 'Month'),
          ],
          onChanged: (value) {
            setState(() => _period = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
        FilterDropdown<bool>(
          value: _ongoingOnly,
          width: 160,
          options: const [
            FilterOption<bool>(label: 'All shifts', value: null),
            FilterOption<bool>(label: 'Ongoing only', value: true),
          ],
          onChanged: (value) {
            setState(() => _ongoingOnly = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
      ],
      columns: [
        CrudColumn(
          label: 'Driver',
          value: (s) => s.driverName ?? '',
          maxWidth: 180,
        ),
        CrudColumn(
          label: 'Started',
          value: (s) => Formats.moment(s.startedAt),
          sortKey: 'StartedAt',
        ),
        CrudColumn(
          label: 'Ended',
          value: (s) =>
              s.isOngoing == true ? 'Ongoing' : Formats.moment(s.endedAt),
        ),
        CrudColumn(
          label: 'On shift',
          value: (s) => Formats.minutes(s.durationMinutes),
        ),
        CrudColumn(
          label: 'On break',
          value: (s) => Formats.minutes(s.breakMinutes),
        ),
        CrudColumn(
          label: 'Worked',
          value: (s) => Formats.minutes(s.workedMinutes),
        ),
        CrudColumn(
          label: 'Rides handled',
          value: (s) => (s.ordersHandled ?? 0).toString(),
        ),
      ],
    );
  }
}
