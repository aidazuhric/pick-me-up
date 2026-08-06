import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../models/car.dart';
import '../../models/reference_item.dart';
import '../../providers/car_provider.dart';
import '../../providers/reference_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/db_dropdown.dart';
import '../../widgets/labeled_form.dart';

class CarForm extends StatefulWidget {
  const CarForm({
    super.key,
    required this.provider,
    required this.categoryProvider,
    this.existing,
  });

  final CarProvider provider;
  final ReferenceProvider categoryProvider;
  final Car? existing;

  static Future<bool> show(
    BuildContext context, {
    required CarProvider provider,
    required ReferenceProvider categoryProvider,
    Car? existing,
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
            height: 620,
            child: CarForm(
              provider: provider,
              categoryProvider: categoryProvider,
              existing: existing,
            ),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<CarForm> createState() => _CarFormState();
}

class _CarFormState extends State<CarForm> {
  final _formKey = GlobalKey<FormState>();
  final _make = TextEditingController();
  final _model = TextEditingController();
  final _plate = TextEditingController();
  final _year = TextEditingController();
  final _seats = TextEditingController();

  int? _categoryId;
  bool _isActive = true;
  bool _isSaving = false;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();

    final existing = widget.existing;
    if (existing != null) {
      _make.text = existing.make ?? '';
      _model.text = existing.model ?? '';
      _plate.text = existing.plate ?? '';
      _year.text = existing.year?.toString() ?? '';
      _seats.text = existing.seats?.toString() ?? '';
      _isActive = existing.isActive ?? true;
    }
  }

  @override
  void dispose() {
    for (final c in [_make, _model, _plate, _year, _seats]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final request = {
        'make': _make.text.trim(),
        'model': _model.text.trim(),
        'plate': _plate.text.trim(),
        'year': int.parse(_year.text.trim()),
        'seats': int.parse(_seats.text.trim()),
        'vehicleCategoryId': _categoryId,
        'isActive': _isActive,
      };

      final saved = _isEditing
          ? await widget.provider.update(widget.existing!.id!, request)
          : await widget.provider.insert(request);

      if (!mounted) return;

      AppMessages.success(
        context,
        _isEditing
            ? '${saved.description} (${saved.plate}) was saved.'
            : '${saved.description} joined the fleet with plate ${saved.plate}.',
      );

      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  String? _wholeNumber(String? value, String field, int min, int max) {
    final required = Validators.required(value, field);
    if (required != null) return required;

    final parsed = int.tryParse(value!.trim());
    if (parsed == null) {
      return 'Enter $field as a whole number.';
    }
    if (parsed < min || parsed > max) {
      return '$field must be between $min and $max.';
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    return LabeledForm(
      title: _isEditing ? 'Edit vehicle' : 'New vehicle',
      subtitle: _isEditing
          ? 'Registered as ${widget.existing!.plate}'
          : 'The vehicle is added to your own fleet.',
      formKey: _formKey,
      isSaving: _isSaving,
      onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
      onSave: _save,
      fields: [
        LabeledField(
          label: 'Make',
          isRequired: true,
          child: AppTextField(
            controller: _make,
            hintText: 'For example Skoda',
            validator: (v) => Validators.required(v, 'Make'),
          ),
        ),
        LabeledField(
          label: 'Model',
          isRequired: true,
          child: AppTextField(
            controller: _model,
            hintText: 'For example Octavia',
            validator: (v) => Validators.required(v, 'Model'),
          ),
        ),
        LabeledField(
          label: 'Registration plate',
          isRequired: true,
          child: AppTextField(
            controller: _plate,
            hintText: 'For example A12-T-345',
            validator: (v) => Validators.required(v, 'Registration plate'),
          ),
        ),
        LabeledField(
          label: 'Year',
          isRequired: true,
          child: AppTextField(
            controller: _year,
            keyboardType: TextInputType.number,
            validator: (v) => _wholeNumber(v, 'Year', 1980, DateTime.now().year + 1),
          ),
        ),
        LabeledField(
          label: 'Seats',
          isRequired: true,
          child: AppTextField(
            controller: _seats,
            keyboardType: TextInputType.number,
            validator: (v) => _wholeNumber(v, 'Seats', 1, 9),
          ),
        ),
        LabeledField(
          label: 'Category',
          isRequired: true,
          hint: 'Loaded from the database',
          child: DbDropdown<ReferenceItem>(
            loadOptions: () async {
              final result = await widget.categoryProvider.get(
                filter: {'PageSize': 100, 'IsActive': true},
              );
              return result.items ?? const [];
            },
            labelOf: (c) => c.name ?? '',
            idOf: (c) => c.id ?? 0,
            value: _categoryId,
            emptyMessage:
                'No vehicle categories exist yet. An administrator has to add one first.',
            onChanged: (value) => setState(() => _categoryId = value),
            validator: (v) => Validators.chosen(v, 'a category'),
          ),
        ),
        LabeledField(
          label: 'In service',
          child: AppToggleField(
            value: _isActive,
            onChanged: (value) => setState(() => _isActive = value),
            description: _isActive
                ? 'Can be assigned to a driver and take rides.'
                : 'Kept in the fleet but not available for rides.',
          ),
        ),
      ],
    );
  }
}
