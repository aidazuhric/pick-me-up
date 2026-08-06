import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../models/search_result.dart';
import '../utils/validators.dart';
import '../widgets/app_messages.dart';
import '../widgets/confirm_dialog.dart';
import '../widgets/crud_data_table.dart';
import '../widgets/db_dropdown.dart';
import '../widgets/disabled_action.dart';
import '../widgets/labeled_form.dart';
import '../widgets/map_picker_modal.dart';

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

  Future<SearchResult<_Sample>> _load(
      int page, int size, String? search, String? sort) async {
    final all = [
      const _Sample(1, 'Mostar Taxi', 'Mostar'),
      const _Sample(2, 'Neretva Prevoz', 'Mostar'),
      const _Sample(3, 'Sarajevo Express', 'Sarajevo'),
    ];

    final filtered = search == null
        ? all
        : all
            .where((s) => s.name.toLowerCase().contains(search.toLowerCase()))
            .toList();

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
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Component showcase', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'The shared building blocks every Admin and Company screen is assembled from.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.lg),
        Expanded(
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _Section(
                  label: 'Data table',
                  caption:
                      'Search and sort grouped with the table they control, row actions on the right, paging and ADD NEW in the footer.',
                  child: SizedBox(height: 340, child: _buildTable()),
                ),
                const SizedBox(height: AppSpacing.lg),
                _Section(
                  label: 'Entity form',
                  caption:
                      'Labels on the left, fields on a fixed column, validation messages below each control.',
                  child: SizedBox(height: 560, child: _buildForm()),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildTable() {
    return CrudDataTable<_Sample>(
      title: 'Taxi companies',
      subtitle: 'Sample rows — this table is not connected to the API.',
      columns: [
        CrudColumn(label: 'Company name', value: (s) => s.name, sortKey: 'Name'),
        CrudColumn(label: 'City', value: (s) => s.city, sortKey: 'City'),
      ],
      loadPage: _load,
      searchHint: 'Search by company name',
      onCreate: () async {
        _announce('ADD NEW opens the create form.');
        return false;
      },
      onEdit: (s) async {
        _announce('Edit opens ${s.name} in the form.');
        return false;
      },
      onDelete: (s) async {
        final ok = await ConfirmDialog.show(
          context,
          title: 'Remove ${s.name}?',
          message: 'The company stops appearing for passengers.',
          confirmLabel: 'DELETE',
          isDestructive: true,
        );
        if (ok) {
          _announce('${s.name} would be removed.');
        }
        return false;
      },
      deleteGuard: (s) => s.id == 1
          ? const RowGuard(
              isAllowed: false,
              reason: 'This company still has active orders.',
            )
          : RowGuard.allowed,
    );
  }

  Widget _buildForm() {
    return LabeledForm(
      title: 'Add taxi company',
      subtitle: 'Every field below is wired to the same validation rules as the API.',
      formKey: _formKey,
      onBack: () => _announce('Back and X both return to the list.'),
      onSave: () {
        if (_formKey.currentState!.validate()) {
          _announce('Mostar Taxi was added in Mostar.');
        }
      },
      fields: [
        LabeledField(
          label: 'Company name',
          isRequired: true,
          child: AppTextField(
            controller: _nameController,
            hintText: 'For example Mostar Taxi',
            validator: (v) => Validators.required(v, 'Company name'),
          ),
        ),
        LabeledField(
          label: 'Contact email',
          isRequired: true,
          child: AppTextField(
            controller: _emailController,
            hintText: 'name@company.com',
            validator: Validators.email,
          ),
        ),
        LabeledField(
          label: 'City',
          isRequired: true,
          hint: 'Loaded from the database',
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
          label: 'Available',
          child: AppToggleField(
            value: _isActive,
            onChanged: (v) => setState(() => _isActive = v),
            description: 'Visible to passengers when ordering a ride.',
          ),
        ),
        LabeledField(
          label: 'Operating since',
          child: AppDateField(
            value: _date,
            onChanged: (v) => setState(() => _date = v),
          ),
        ),
        LabeledField(
          label: 'Headquarters',
          hint: 'Coordinates come from the map',
          isWide: true,
          child: Row(
            children: [
              OutlinedButton.icon(
                onPressed: () => MapPickerModal.show(context),
                icon: const Icon(Icons.map_outlined, size: 16),
                label: const Text('Pick on the map'),
              ),
              const SizedBox(width: AppSpacing.md),
              const DisabledAction(
                isEnabled: false,
                reason: 'Coordinates are chosen on the map, never typed.',
                child: Text('Type coordinates'),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _Section extends StatelessWidget {
  const _Section({
    required this.label,
    required this.caption,
    required this.child,
  });

  final String label;
  final String caption;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            Container(
              width: 3,
              height: 16,
              decoration: BoxDecoration(
                color: AppColors.primaryIndigo,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Text(label.toUpperCase(), style: theme.textTheme.titleSmall),
          ],
        ),
        const SizedBox(height: AppSpacing.xs),
        Padding(
          padding: const EdgeInsets.only(left: 11),
          child: Text(caption, style: theme.textTheme.bodySmall),
        ),
        const SizedBox(height: AppSpacing.md),
        child,
      ],
    );
  }
}
