import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../models/car.dart';
import '../../models/car_assignment.dart';
import '../../models/driver.dart';
import '../../providers/car_assignment_provider.dart';
import '../../providers/car_provider.dart';
import '../../providers/driver_provider.dart';
import '../../utils/api_dates.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/db_dropdown.dart';
import '../../widgets/labeled_form.dart';

class CarAssignmentForm extends StatefulWidget {
  const CarAssignmentForm({
    super.key,
    required this.provider,
    required this.carProvider,
    required this.driverProvider,
    this.existing,
  });

  final CarAssignmentProvider provider;
  final CarProvider carProvider;
  final DriverProvider driverProvider;
  final CarAssignment? existing;

  static Future<bool> show(
    BuildContext context, {
    required CarAssignmentProvider provider,
    required CarProvider carProvider,
    required DriverProvider driverProvider,
    CarAssignment? existing,
  }) async {
    final saved = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 760,
            height: 520,
            child: CarAssignmentForm(
              provider: provider,
              carProvider: carProvider,
              driverProvider: driverProvider,
              existing: existing,
            ),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<CarAssignmentForm> createState() => _CarAssignmentFormState();
}

class _CarAssignmentFormState extends State<CarAssignmentForm> {
  final _formKey = GlobalKey<FormState>();

  int? _carId;
  int? _driverId;
  DateTime _assignedFrom = DateTime.now();
  DateTime? _assignedTo;
  bool _isSaving = false;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();

    final existing = widget.existing;
    if (existing != null) {
      _assignedFrom = existing.assignedFrom ?? DateTime.now();
      _assignedTo = existing.assignedTo;
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    if (_assignedTo != null && !_assignedTo!.isAfter(_assignedFrom)) {
      AppMessages.failure(
        context,
        'The end of the assignment must be after its start.',
      );
      return;
    }

    setState(() => _isSaving = true);

    try {
      final request = <String, dynamic>{
        'assignedFrom': ApiDates.dayToUtc(_assignedFrom),
        'assignedTo': ApiDates.dayToUtcOrNull(_assignedTo),
        'isActive': true,
      };

      if (!_isEditing) {
        request['carId'] = _carId;
        request['driverId'] = _driverId;
      }

      final saved = _isEditing
          ? await widget.provider.update(widget.existing!.id!, request)
          : await widget.provider.insert(request);

      if (!mounted) return;

      AppMessages.success(
        context,
        _isEditing
            ? '${saved.carPlate} and ${saved.driverName} were saved.'
            : '${saved.carPlate} is assigned to ${saved.driverName}.',
      );

      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return LabeledForm(
      title: _isEditing ? 'Edit assignment' : 'New assignment',
      subtitle: _isEditing
          ? '${widget.existing!.carPlate} · ${widget.existing!.driverName}'
          : 'Only your own vehicles and drivers can be paired.',
      formKey: _formKey,
      isSaving: _isSaving,
      onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
      onSave: _save,
      fields: [
        if (!_isEditing) ...[
          LabeledField(
            label: 'Vehicle',
            isRequired: true,
            hint: 'Your fleet only',
            child: DbDropdown<Car>(
              loadOptions: () async {
                final result = await widget.carProvider.get(
                  filter: {'PageSize': 100, 'IsActive': true},
                );
                return result.items ?? const [];
              },
              labelOf: (car) => '${car.plate} · ${car.description}',
              idOf: (car) => car.id ?? 0,
              value: _carId,
              emptyMessage:
                  'Your fleet has no vehicles yet. Add one under Vehicles first.',
              onChanged: (value) => setState(() => _carId = value),
              validator: (v) => Validators.chosen(v, 'a vehicle'),
            ),
          ),
          LabeledField(
            label: 'Driver',
            isRequired: true,
            hint: 'Your drivers only',
            child: DbDropdown<Driver>(
              loadOptions: () async {
                final result = await widget.driverProvider.get(
                  filter: {'PageSize': 100, 'IsActive': true},
                );
                return result.items ?? const [];
              },
              labelOf: (driver) => driver.fullName ?? '',
              idOf: (driver) => driver.id ?? 0,
              value: _driverId,
              emptyMessage:
                  'You have no drivers yet. Add one under Drivers first.',
              onChanged: (value) => setState(() => _driverId = value),
              validator: (v) => Validators.chosen(v, 'a driver'),
            ),
          ),
        ],
        LabeledField(
          label: 'Assigned from',
          isRequired: true,
          child: AppDateField(
            value: _assignedFrom,
            onChanged: (value) => setState(() => _assignedFrom = value),
          ),
        ),
        LabeledField(
          label: 'Assigned to',
          hint: 'Leave empty for an open-ended assignment',
          child: Row(
            children: [
              Expanded(
                child: AppDateField(
                  value: _assignedTo,
                  onChanged: (value) => setState(() => _assignedTo = value),
                ),
              ),
              if (_assignedTo != null) ...[
                const SizedBox(width: AppSpacing.sm),
                TextButton(
                  onPressed: () => setState(() => _assignedTo = null),
                  child: const Text('Clear'),
                ),
              ],
            ],
          ),
        ),
      ],
    );
  }
}
