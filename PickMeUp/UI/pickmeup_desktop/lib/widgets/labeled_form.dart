import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

class LabeledForm extends StatelessWidget {
  const LabeledForm({
    super.key,
    required this.title,
    required this.formKey,
    required this.fields,
    required this.onSave,
    this.onBack,
    this.isSaving = false,
    this.saveLabel = 'SAVE',
  });

  final String title;
  final GlobalKey<FormState> formKey;
  final List<LabeledField> fields;
  final VoidCallback onSave;
  final VoidCallback? onBack;
  final bool isSaving;
  final String saveLabel;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Row(
          children: [
            if (onBack != null)
              TextButton.icon(
                onPressed: isSaving ? null : onBack,
                icon: const Icon(Icons.arrow_back),
                label: const Text('Back'),
              ),
            const Spacer(),
            Text(title, style: theme.textTheme.headlineSmall),
            const Spacer(),
            IconButton(
              tooltip: 'Close',
              onPressed: isSaving ? null : onBack,
              icon: const Icon(Icons.close),
            ),
          ],
        ),
        const Divider(height: 24),
        Expanded(
          child: SingleChildScrollView(
            child: Form(
              key: formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  for (final field in fields) _FieldRow(field: field),
                ],
              ),
            ),
          ),
        ),
        const SizedBox(height: 12),
        Align(
          alignment: Alignment.centerRight,
          child: FilledButton(
            onPressed: isSaving ? null : onSave,
            child: isSaving
                ? const SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                  )
                : Text(saveLabel),
          ),
        ),
      ],
    );
  }
}

class LabeledField {
  const LabeledField({
    required this.label,
    required this.child,
    this.isRequired = false,
  });

  final String label;
  final Widget child;
  final bool isRequired;
}

class _FieldRow extends StatelessWidget {
  const _FieldRow({required this.field});

  final LabeledField field;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 220,
            child: Padding(
              padding: const EdgeInsets.only(top: 16, right: 16),
              child: RichText(
                text: TextSpan(
                  text: field.label,
                  style: theme.textTheme.bodyMedium?.copyWith(fontWeight: FontWeight.w600),
                  children: field.isRequired
                      ? [
                          TextSpan(
                            text: ' *',
                            style: TextStyle(color: theme.colorScheme.error),
                          ),
                        ]
                      : const [],
                ),
              ),
            ),
          ),
          Expanded(child: field.child),
        ],
      ),
    );
  }
}

class AppTextField extends StatelessWidget {
  const AppTextField({
    super.key,
    required this.controller,
    this.hintText,
    this.validator,
    this.keyboardType,
    this.obscureText = false,
    this.onSubmitted,
  });

  final TextEditingController controller;
  final String? hintText;
  final String? Function(String?)? validator;
  final TextInputType? keyboardType;
  final bool obscureText;
  final VoidCallback? onSubmitted;

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      decoration: InputDecoration(hintText: hintText),
      validator: validator,
      keyboardType: keyboardType,
      obscureText: obscureText,
      onFieldSubmitted: onSubmitted == null ? null : (_) => onSubmitted!(),
      autovalidateMode: AutovalidateMode.onUserInteraction,
    );
  }
}

class AppToggleField extends StatelessWidget {
  const AppToggleField({
    super.key,
    required this.value,
    required this.onChanged,
    required this.description,
  });

  final bool value;
  final ValueChanged<bool> onChanged;
  final String description;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Switch(
          value: value,
          activeTrackColor: AppColors.toggleOn,
          onChanged: onChanged,
        ),
        const SizedBox(width: 12),
        Expanded(child: Text(description)),
      ],
    );
  }
}

class AppDateField extends StatefulWidget {
  const AppDateField({
    super.key,
    required this.value,
    required this.onChanged,
    this.firstDate,
    this.lastDate,
  });

  final DateTime? value;
  final ValueChanged<DateTime> onChanged;
  final DateTime? firstDate;
  final DateTime? lastDate;

  @override
  State<AppDateField> createState() => _AppDateFieldState();
}

class _AppDateFieldState extends State<AppDateField> {
  Future<void> _pick() async {
    final now = DateTime.now();

    final picked = await showDatePicker(
      context: context,
      initialDate: widget.value ?? now,
      firstDate: widget.firstDate ?? DateTime(now.year - 30),
      lastDate: widget.lastDate ?? DateTime(now.year + 5),
    );

    if (picked != null) {
      widget.onChanged(picked);
    }
  }

  @override
  Widget build(BuildContext context) {
    final label = widget.value == null
        ? 'Choose a date'
        : '${widget.value!.year}-${widget.value!.month.toString().padLeft(2, '0')}-${widget.value!.day.toString().padLeft(2, '0')}';

    return OutlinedButton.icon(
      onPressed: _pick,
      icon: const Icon(Icons.calendar_today, size: 18),
      label: Align(alignment: Alignment.centerLeft, child: Text(label)),
    );
  }
}
