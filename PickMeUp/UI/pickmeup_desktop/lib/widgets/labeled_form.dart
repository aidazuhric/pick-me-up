import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';

class LabeledForm extends StatelessWidget {
  const LabeledForm({
    super.key,
    required this.title,
    required this.formKey,
    required this.fields,
    required this.onSave,
    this.subtitle,
    this.onBack,
    this.isSaving = false,
    this.saveLabel = 'SAVE',
  });

  final String title;
  final String? subtitle;
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
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (onBack != null) ...[
              IconButton(
                tooltip: 'Back',
                onPressed: isSaving ? null : onBack,
                icon: const Icon(Icons.arrow_back, size: 20),
              ),
              const SizedBox(width: AppSpacing.sm),
            ],
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(title, style: theme.textTheme.headlineSmall),
                  if (subtitle != null) ...[
                    const SizedBox(height: 2),
                    Text(subtitle!, style: theme.textTheme.bodySmall),
                  ],
                ],
              ),
            ),
            if (onBack != null)
              IconButton(
                tooltip: 'Close',
                onPressed: isSaving ? null : onBack,
                icon: const Icon(Icons.close, size: 20),
              ),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        const Divider(height: 1),
        Expanded(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(vertical: AppSpacing.lg),
            child: Align(
              alignment: Alignment.topLeft,
              child: ConstrainedBox(
                constraints:
                    const BoxConstraints(maxWidth: AppSpacing.formMaxWidth),
                child: Form(
                  key: formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      for (var i = 0; i < fields.length; i++) ...[
                        if (i > 0) const SizedBox(height: AppSpacing.md),
                        _FieldRow(field: fields[i]),
                      ],
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
        const Divider(height: 1),
        Padding(
          padding: const EdgeInsets.only(top: AppSpacing.md),
          child: Align(
            alignment: Alignment.centerLeft,
            child: ConstrainedBox(
              constraints:
                  const BoxConstraints(maxWidth: AppSpacing.formMaxWidth),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  if (onBack != null) ...[
                    TextButton(
                      onPressed: isSaving ? null : onBack,
                      child: const Text('CANCEL'),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                  ],
                  FilledButton(
                    onPressed: isSaving ? null : onSave,
                    child: isSaving
                        ? const SizedBox(
                            height: 18,
                            width: 18,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : Text(saveLabel),
                  ),
                ],
              ),
            ),
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
    this.hint,
    this.isWide = false,
  });

  final String label;
  final Widget child;
  final bool isRequired;
  final String? hint;
  final bool isWide;
}

class _FieldRow extends StatelessWidget {
  const _FieldRow({required this.field});

  final LabeledField field;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: AppSpacing.formLabelWidth,
          child: Padding(
            padding: const EdgeInsets.only(top: 12, right: AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                RichText(
                  text: TextSpan(
                    text: field.label,
                    style: theme.textTheme.bodyMedium
                        ?.copyWith(fontWeight: FontWeight.w600),
                    children: field.isRequired
                        ? const [
                            TextSpan(
                              text: ' *',
                              style: TextStyle(color: AppColors.danger),
                            ),
                          ]
                        : const [],
                  ),
                ),
                if (field.hint != null) ...[
                  const SizedBox(height: 2),
                  Text(field.hint!, style: theme.textTheme.bodySmall),
                ],
              ],
            ),
          ),
        ),
        Expanded(
          child: Align(
            alignment: Alignment.centerLeft,
            child: ConstrainedBox(
              constraints: BoxConstraints(
                maxWidth: field.isWide
                    ? double.infinity
                    : AppSpacing.fieldMaxWidth,
              ),
              child: field.child,
            ),
          ),
        ),
      ],
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
    this.maxLines = 1,
  });

  final TextEditingController controller;
  final String? hintText;
  final String? Function(String?)? validator;
  final TextInputType? keyboardType;
  final bool obscureText;
  final VoidCallback? onSubmitted;
  final int maxLines;

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      style: Theme.of(context).textTheme.bodyMedium,
      decoration: InputDecoration(hintText: hintText),
      validator: validator,
      keyboardType: keyboardType,
      obscureText: obscureText,
      maxLines: obscureText ? 1 : maxLines,
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
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.xs),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Switch(value: value, onChanged: onChanged),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              description,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        ],
      ),
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
    final theme = Theme.of(context);
    final hasValue = widget.value != null;

    final label = hasValue
        ? '${widget.value!.year}-${widget.value!.month.toString().padLeft(2, '0')}-${widget.value!.day.toString().padLeft(2, '0')}'
        : 'Choose a date';

    return InkWell(
      onTap: _pick,
      borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      child: InputDecorator(
        decoration: const InputDecoration(),
        child: Row(
          children: [
            const Icon(Icons.calendar_today_outlined,
                size: 16, color: AppColors.textMuted),
            const SizedBox(width: AppSpacing.sm),
            Text(
              label,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: hasValue ? AppColors.textPrimary : AppColors.textMuted,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
