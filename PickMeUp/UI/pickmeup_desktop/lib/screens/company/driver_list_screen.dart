import 'package:flutter/material.dart';

import '../../models/driver.dart';
import '../../models/search_result.dart';
import '../../providers/driver_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/confirm_dialog.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'driver_form.dart';

class DriverListScreen extends StatefulWidget {
  const DriverListScreen({super.key});

  @override
  State<DriverListScreen> createState() => _DriverListScreenState();
}

class _DriverListScreenState extends State<DriverListScreen> {
  final _provider = DriverProvider();
  final _tableKey = GlobalKey<CrudDataTableState<Driver>>();

  bool _includeResigned = false;

  Future<SearchResult<Driver>> _load(
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
      'Name': ?search,
      'IncludeResigned': _includeResigned,
    });
  }

  Future<bool> _create() => DriverForm.show(context, provider: _provider);

  Future<bool> _edit(Driver driver) =>
      DriverForm.show(context, provider: _provider, existing: driver);

  Future<bool> _resign(Driver driver) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Record ${driver.fullName} as resigned?',
      message:
          'They stop receiving ride offers straight away. Their shifts and completed rides are kept, and the record can still be removed later.',
      confirmLabel: 'RESIGN',
    );

    if (!confirmed) return false;

    try {
      final resigned = await _provider.resign(driver.id!);
      if (!mounted) return false;
      AppMessages.success(
        context,
        '${resigned.fullName} is recorded as resigned.',
      );
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  Future<bool> _delete(Driver driver) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Remove ${driver.fullName} from the fleet?',
      message:
          'The employment record is removed. A driver on an active shift or mid-ride cannot be removed — record them as resigned first.',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) return false;

    try {
      await _provider.remove(driver.id!);
      if (!mounted) return false;
      AppMessages.success(context, '${driver.fullName} was removed.');
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  static String _day(DateTime? value) {
    if (value == null) return '';
    final local = value.toLocal();
    String two(int v) => v.toString().padLeft(2, '0');
    return '${local.year}-${two(local.month)}-${two(local.day)}';
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<Driver>(
      key: _tableKey,
      title: 'Drivers',
      subtitle: 'The drivers employed by your company.',
      searchHint: 'Search by driver name',
      loadPage: _load,
      onCreate: _create,
      onEdit: _edit,
      onDelete: _delete,
      rowActions: [
        RowAction<Driver>(
          tooltip: 'Record as resigned',
          icon: Icons.logout,
          onPressed: _resign,
          guard: (driver) => driver.hasResigned
              ? const RowGuard(
                  isAllowed: false,
                  reason: 'This driver has already resigned.',
                )
              : RowGuard.allowed,
        ),
      ],
      filters: [
        FilterDropdown<bool>(
          value: _includeResigned,
          width: 190,
          options: const [
            FilterOption<bool>(label: 'Current drivers', value: false),
            FilterOption<bool>(label: 'Including resigned', value: true),
          ],
          onChanged: (value) {
            setState(() => _includeResigned = value ?? false);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
      ],
      columns: [
        CrudColumn(
          label: 'Driver',
          value: (d) => d.fullName ?? '',
          sortKey: 'Id',
          maxWidth: 200,
        ),
        CrudColumn(label: 'Email', value: (d) => d.email ?? '', maxWidth: 220),
        CrudColumn(label: 'Phone', value: (d) => d.phoneNumber ?? ''),
        CrudColumn(label: 'Hired', value: (d) => _day(d.hireDate)),
        CrudColumn(
          label: 'Status',
          value: (d) => d.hasResigned
              ? 'Resigned ${_day(d.resignedAt)}'
              : (d.isActive == true ? 'Available' : 'Not taking work'),
          maxWidth: 180,
        ),
      ],
    );
  }
}
