import 'package:flutter/material.dart';

import '../models/search_result.dart';
import '../widgets/app_messages.dart';
import '../widgets/confirm_dialog.dart';
import '../widgets/crud_data_table.dart';
import '../widgets/db_dropdown.dart';
import '../widgets/disabled_action.dart';
import '../widgets/labeled_form.dart';
import '../widgets/map_picker_modal.dart';
import '../utils/validators.dart';

class _Sample {
  const _Sample(this.id, this.name, this.city);

  final int id;
  final String name;
  final String city;
}

class ShellGalleryScreen extends StatefulWidget {
  const ShellGalleryScreen({super.key});

  @override
  State<ShellGalleryScreen> createState() => _ShellGalleryScreenState();
}

class _ShellGalleryScreenState extends State<ShellGalleryScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();

  bool _isActive = true;
  DateTime? _date;
  int? _cityId;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<SearchResult<_Sample>> _load(int page, int size, String? search, String? sort) async {
    final all = [
      const _Sample(1, 'Mostar Taxi', 'Mostar'),
      const _Sample(2, 'Neretva Prevoz', 'Mostar'),
      const _Sample(3, 'Sarajevo Express', 'Sarajevo'),
    ];

    final filtered = search == null
        ? all
        : all.where((s) => s.name.toLowerCase().contains(search.toLowerCase())).toList();

    return SearchResult<_Sample>()
      ..items = filtered
      ..totalCount = filtered.length;
  }

  void _announce(String message) {
    if (!mounted) return;
    AppMessages.success(context, message);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        SizedBox(
          height: 320,
          child: CrudDataTable<_Sample>(
            title: 'CrudDataTable',
            columns: [
              CrudColumn(label: 'Name', value: (s) => s.name, sortKey: 'Name'),
              CrudColumn(label: 'City', value: (s) => s.city, sortKey: 'City'),
            ],
            loadPage: _load,
            onCreate: () async {
              _announce('ADD NEW fires the create flow.');
              return false;
            },
            onEdit: (s) async => false,
            onDelete: (s) async {
              final ok = await ConfirmDialog.show(
                context,
                title: 'Delete ${s.name}?',
                message: 'This cannot be undone.',
                confirmLabel: 'DELETE',
                isDestructive: true,
              );
              if (ok) {
                _announce('${s.name} would be deleted.');
              }
              return false;
            },
            deleteGuard: (s) => s.id == 1
                ? const RowGuard(isAllowed: false, reason: 'This company still has active orders.')
                : RowGuard.allowed,
          ),
        ),
        const Divider(height: 32),
        Expanded(
          child: LabeledForm(
            title: 'LabeledForm',
            formKey: _formKey,
            onBack: () => AppMessages.success(context, 'Back and X both return to the list.'),
            onSave: () {
              if (_formKey.currentState!.validate()) {
                AppMessages.success(context, 'Validation passed.');
              }
            },
            fields: [
              LabeledField(
                label: 'Name',
                isRequired: true,
                child: AppTextField(
                  controller: _nameController,
                  validator: (v) => Validators.required(v, 'Name'),
                ),
              ),
              LabeledField(
                label: 'Contact email',
                isRequired: true,
                child: AppTextField(
                  controller: _emailController,
                  validator: Validators.email,
                ),
              ),
              LabeledField(
                label: 'City',
                isRequired: true,
                child: DbDropdown<_Sample>(
                  loadOptions: () async => const [
                    _Sample(1, 'Mostar', ''),
                    _Sample(2, 'Sarajevo', ''),
                  ],
                  labelOf: (s) => s.name,
                  idOf: (s) => s.id,
                  value: _cityId,
                  onChanged: (v) => setState(() => _cityId = v),
                  validator: (v) => Validators.chosen(v, 'a city'),
                ),
              ),
              LabeledField(
                label: 'Active',
                child: AppToggleField(
                  value: _isActive,
                  onChanged: (v) => setState(() => _isActive = v),
                  description: 'Visible to passengers',
                ),
              ),
              LabeledField(
                label: 'Start date',
                child: AppDateField(value: _date, onChanged: (v) => setState(() => _date = v)),
              ),
              LabeledField(
                label: 'Location',
                child: Row(
                  children: [
                    OutlinedButton.icon(
                      onPressed: () => MapPickerModal.show(context),
                      icon: const Icon(Icons.map_outlined, size: 18),
                      label: const Text('Pick on the map'),
                    ),
                    const SizedBox(width: 16),
                    const DisabledAction(
                      isEnabled: false,
                      reason: 'Coordinates are chosen on the map, never typed.',
                      child: Text('Manual entry'),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
