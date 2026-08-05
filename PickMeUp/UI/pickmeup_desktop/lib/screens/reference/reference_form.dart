import 'package:flutter/material.dart';

import '../../models/reference_item.dart';
import '../../providers/reference_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/labeled_form.dart';
import 'reference_table.dart';

class ReferenceForm extends StatefulWidget {
  const ReferenceForm({
    super.key,
    required this.table,
    required this.provider,
    this.existing,
  });

  final ReferenceTable table;
  final ReferenceProvider provider;
  final ReferenceItem? existing;

  static Future<bool> show(
    BuildContext context, {
    required ReferenceTable table,
    required ReferenceProvider provider,
    ReferenceItem? existing,
  }) async {
    final saved = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(28),
          child: SizedBox(
            width: 720,
            height: table.hasSortOrder ? 460 : 400,
            child: ReferenceForm(
              table: table,
              provider: provider,
              existing: existing,
            ),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<ReferenceForm> createState() => _ReferenceFormState();
}

class _ReferenceFormState extends State<ReferenceForm> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _description = TextEditingController();
  final _postalCode = TextEditingController();
  final _sortOrder = TextEditingController();

  bool _isActive = true;
  bool _isSaving = false;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();

    final existing = widget.existing;
    if (existing != null) {
      _name.text = existing.name ?? '';
      _description.text = existing.description ?? '';
      _postalCode.text = existing.postalCode ?? '';
      _sortOrder.text = existing.sortOrder?.toString() ?? '';
      _isActive = existing.isActive ?? true;
    }
  }

  @override
  void dispose() {
    for (final controller in [_name, _description, _postalCode, _sortOrder]) {
      controller.dispose();
    }
    super.dispose();
  }

  Map<String, dynamic> _buildRequest() {
    final request = <String, dynamic>{
      'name': _name.text.trim(),
      'isActive': _isActive,
    };

    if (widget.table.hasPostalCode) {
      final postalCode = _postalCode.text.trim();
      request['postalCode'] = postalCode.isEmpty ? null : postalCode;
    }

    if (widget.table.hasDescription) {
      final description = _description.text.trim();
      request['description'] = description.isEmpty ? null : description;
    }

    if (widget.table.hasSortOrder) {
      request['sortOrder'] = int.parse(_sortOrder.text.trim());
    }

    return request;
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final request = _buildRequest();

      final saved = _isEditing
          ? await widget.provider.update(widget.existing!.id!, request)
          : await widget.provider.insert(request);

      if (!mounted) return;

      AppMessages.success(
        context,
        _isEditing
            ? '${saved.name} was saved.'
            : '${saved.name} ${widget.table.createdMessage}',
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
    final table = widget.table;

    return LabeledForm(
      title: _isEditing ? 'Edit ${table.itemNoun}' : 'New ${table.itemNoun}',
      formKey: _formKey,
      isSaving: _isSaving,
      onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
      onSave: _save,
      fields: [
        LabeledField(
          label: table.nameLabel,
          isRequired: true,
          child: AppTextField(
            controller: _name,
            hintText: 'For example ${_nameExample(table)}',
            validator: (value) =>
                Validators.required(value, table.nameLabel) ??
                Validators.maxLength(value, table.nameMaxLength, table.nameLabel),
          ),
        ),
        if (table.hasPostalCode)
          LabeledField(
            label: 'Postal code',
            child: AppTextField(
              controller: _postalCode,
              hintText: 'For example 88000',
              validator: (value) => Validators.maxLength(
                value,
                ReferenceTable.postalCodeMaxLength,
                'Postal code',
              ),
            ),
          ),
        if (table.hasDescription)
          LabeledField(
            label: 'Description',
            child: AppTextField(
              controller: _description,
              hintText: 'What this ${table.itemNoun} means',
              validator: (value) => Validators.maxLength(
                value,
                ReferenceTable.descriptionMaxLength,
                'Description',
              ),
            ),
          ),
        if (table.hasSortOrder)
          LabeledField(
            label: 'Display order',
            isRequired: true,
            child: AppTextField(
              controller: _sortOrder,
              hintText: 'Lower numbers are listed first, for example 1',
              keyboardType: TextInputType.number,
              validator: _sortOrderValidator,
            ),
          ),
        LabeledField(
          label: 'Available',
          child: AppToggleField(
            value: _isActive,
            onChanged: (value) => setState(() => _isActive = value),
            description: _isActive
                ? 'Shown wherever a ${table.itemNoun} can be chosen.'
                : 'Hidden from new records, existing ones keep it.',
          ),
        ),
      ],
    );
  }

  String? _sortOrderValidator(String? value) {
    final required = Validators.required(value, 'Display order');
    if (required != null) {
      return required;
    }

    final parsed = int.tryParse(value!.trim());
    if (parsed == null) {
      return 'Enter the display order as a whole number, for example 1.';
    }
    if (parsed < 0) {
      return 'Display order must be zero or greater.';
    }
    return null;
  }

  static String _nameExample(ReferenceTable table) {
    if (table.hasPostalCode) {
      return 'Mostar';
    }
    if (table.hasSortOrder) {
      return 'Pending';
    }
    return 'Standard';
  }
}
