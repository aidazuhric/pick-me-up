import 'package:flutter/material.dart';

import '../../models/car_assignment.dart';
import '../../models/search_result.dart';
import '../../providers/car_assignment_provider.dart';
import '../../providers/car_provider.dart';
import '../../providers/driver_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/confirm_dialog.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'car_assignment_form.dart';

class CarAssignmentListScreen extends StatefulWidget {
  const CarAssignmentListScreen({super.key});

  @override
  State<CarAssignmentListScreen> createState() =>
      _CarAssignmentListScreenState();
}

class _CarAssignmentListScreenState extends State<CarAssignmentListScreen> {
  final _provider = CarAssignmentProvider();
  final _carProvider = CarProvider();
  final _driverProvider = DriverProvider();
  final _tableKey = GlobalKey<CrudDataTableState<CarAssignment>>();

  bool? _ongoingOnly;

  Future<SearchResult<CarAssignment>> _load(
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
      'Plate': ?search,
      'OngoingOnly': ?_ongoingOnly,
    });
  }

  Future<bool> _create() => CarAssignmentForm.show(
        context,
        provider: _provider,
        carProvider: _carProvider,
        driverProvider: _driverProvider,
      );

  Future<bool> _edit(CarAssignment assignment) => CarAssignmentForm.show(
        context,
        provider: _provider,
        carProvider: _carProvider,
        driverProvider: _driverProvider,
        existing: assignment,
      );

  Future<bool> _close(CarAssignment assignment) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'End this assignment now?',
      message:
          '${assignment.carPlate} and ${assignment.driverName} are freed for reassignment from today. The record of the assignment is kept.',
      confirmLabel: 'END NOW',
    );

    if (!confirmed) return false;

    try {
      final closed = await _provider.close(assignment.id!);
      if (!mounted) return false;
      AppMessages.success(
        context,
        '${closed.carPlate} is no longer assigned to ${closed.driverName}.',
      );
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  Future<bool> _delete(CarAssignment assignment) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Remove this assignment?',
      message:
          'The pairing of ${assignment.carPlate} with ${assignment.driverName} is deleted. To keep the history instead, end it with the clock action.',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) return false;

    try {
      await _provider.remove(assignment.id!);
      if (!mounted) return false;
      AppMessages.success(context, 'The assignment was removed.');
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
    return CrudDataTable<CarAssignment>(
      key: _tableKey,
      title: 'Assignments',
      subtitle: 'Which of your drivers is driving which of your vehicles.',
      searchHint: 'Search by registration plate',
      loadPage: _load,
      onCreate: _create,
      onEdit: _edit,
      onDelete: _delete,
      rowActions: [
        RowAction<CarAssignment>(
          tooltip: 'End this assignment now',
          icon: Icons.schedule,
          onPressed: _close,
          guard: (a) => a.isOngoing == true
              ? RowGuard.allowed
              : const RowGuard(
                  isAllowed: false,
                  reason: 'This assignment has already ended.',
                ),
        ),
      ],
      filters: [
        FilterDropdown<bool>(
          value: _ongoingOnly,
          width: 180,
          options: const [
            FilterOption<bool>(label: 'All assignments', value: null),
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
          label: 'Vehicle',
          value: (a) => '${a.carPlate} · ${a.carDescription}',
          maxWidth: 240,
        ),
        CrudColumn(label: 'Category', value: (a) => a.categoryName ?? ''),
        CrudColumn(
          label: 'Driver',
          value: (a) => a.driverName ?? '',
          maxWidth: 180,
        ),
        CrudColumn(label: 'From', value: (a) => _day(a.assignedFrom)),
        CrudColumn(
          label: 'Until',
          value: (a) => a.assignedTo == null ? 'Open-ended' : _day(a.assignedTo),
        ),
      ],
    );
  }
}
