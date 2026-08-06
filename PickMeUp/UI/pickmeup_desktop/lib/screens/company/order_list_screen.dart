import 'package:flutter/material.dart';

import '../../models/order.dart';
import '../../models/search_result.dart';
import '../../providers/order_provider.dart';
import '../../utils/formats.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'order_detail.dart';

class OrderListScreen extends StatefulWidget {
  const OrderListScreen({super.key});

  @override
  State<OrderListScreen> createState() => _OrderListScreenState();
}

class _OrderListScreenState extends State<OrderListScreen> {
  final _provider = OrderProvider();
  final _tableKey = GlobalKey<CrudDataTableState<Order>>();

  String? _status;
  String? _period;

  Future<SearchResult<Order>> _load(
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
      'PassengerName': ?search,
      'Status': ?_status,
      'Period': ?_period,
    });
  }

  Future<bool> _open(Order order) async {
    await OrderDetail.show(context, order: order);
    return false;
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<Order>(
      key: _tableKey,
      title: 'Orders',
      subtitle: 'Every ride your company has been asked to serve.',
      searchHint: 'Search by passenger name',
      loadPage: _load,
      onEdit: _open,
      editTooltip: 'Open ride',
      editIcon: Icons.open_in_new,
      filters: [
        FilterDropdown<String>(
          value: _status,
          width: 170,
          options: const [
            FilterOption<String>(label: 'Any status', value: null),
            FilterOption<String>(label: 'Pending', value: 'Pending'),
            FilterOption<String>(label: 'Confirmed', value: 'Confirmed'),
            FilterOption<String>(label: 'In progress', value: 'InProgress'),
            FilterOption<String>(label: 'Completed', value: 'Completed'),
            FilterOption<String>(label: 'Cancelled', value: 'Cancelled'),
          ],
          onChanged: (value) {
            setState(() => _status = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
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
      ],
      columns: [
        CrudColumn(
          label: 'Passenger',
          value: (o) => o.passengerName ?? '',
          maxWidth: 150,
        ),
        CrudColumn(label: 'Route', value: (o) => o.route, maxWidth: 260),
        CrudColumn(
          label: 'Driver',
          value: (o) => o.driverName ?? 'Not assigned',
          maxWidth: 150,
        ),
        CrudColumn(
          label: 'Status',
          value: (o) => Formats.statusLabel(o.status),
        ),
        CrudColumn(label: 'Fare', value: (o) => Formats.money(o.fare)),
        CrudColumn(
          label: 'Placed',
          value: (o) => Formats.day(o.createdAt),
          sortKey: 'CreatedAt',
        ),
      ],
    );
  }
}
