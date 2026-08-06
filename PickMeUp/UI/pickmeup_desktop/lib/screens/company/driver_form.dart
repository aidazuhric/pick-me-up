import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../models/driver.dart';
import '../../providers/driver_provider.dart';
import '../../utils/api_dates.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/labeled_form.dart';

class DriverForm extends StatefulWidget {
  const DriverForm({super.key, required this.provider, this.existing});

  final DriverProvider provider;
  final Driver? existing;

  static Future<bool> show(
    BuildContext context, {
    required DriverProvider provider,
    Driver? existing,
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
            height: existing == null ? 620 : 520,
            child: DriverForm(provider: provider, existing: existing),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<DriverForm> createState() => _DriverFormState();
}

class _DriverFormState extends State<DriverForm> {
  final _formKey = GlobalKey<FormState>();
  final _firstName = TextEditingController();
  final _lastName = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _username = TextEditingController();

  DateTime _hireDate = DateTime.now();
  bool _isActive = true;
  bool _isSaving = false;

  bool get _isEditing => widget.existing != null;

  @override
  void initState() {
    super.initState();

    final existing = widget.existing;
    if (existing != null) {
      final parts = (existing.fullName ?? '').split(' ');
      _firstName.text = parts.isNotEmpty ? parts.first : '';
      _lastName.text = parts.length > 1 ? parts.sublist(1).join(' ') : '';
      _phone.text = existing.phoneNumber ?? '';
      _hireDate = existing.hireDate ?? DateTime.now();
      _isActive = existing.isActive ?? true;
    }
  }

  @override
  void dispose() {
    for (final c in [_firstName, _lastName, _email, _phone, _username]) {
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
      if (_isEditing) {
        final saved = await widget.provider.update(widget.existing!.id!, {
          'firstName': _firstName.text.trim(),
          'lastName': _lastName.text.trim(),
          'phoneNumber':
              _phone.text.trim().isEmpty ? null : _phone.text.trim(),
          'hireDate': ApiDates.dayToUtc(_hireDate),
          'isActive': _isActive,
        });

        if (!mounted) return;
        AppMessages.success(context, '${saved.fullName} was saved.');
      } else {
        final created = await widget.provider.insert({
          'firstName': _firstName.text.trim(),
          'lastName': _lastName.text.trim(),
          'email': _email.text.trim(),
          'phoneNumber':
              _phone.text.trim().isEmpty ? null : _phone.text.trim(),
          'hireDate': ApiDates.dayToUtc(_hireDate),
          'isActive': _isActive,
          'loginUsername': _username.text.trim(),
        });

        if (!mounted) return;
        _announceCreated(created);
      }

      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  void _announceCreated(Driver created) {
    final password = created.initialLoginPassword;

    AppMessages.success(
      context,
      password == null
          ? '${created.fullName} joined the fleet.'
          : '${created.fullName} joined the fleet. They sign in as '
              '${created.initialLoginUsername} with the temporary password $password — '
              'hand it over now, it is shown only this once.',
    );
  }

  @override
  Widget build(BuildContext context) {
    return LabeledForm(
      title: _isEditing ? 'Edit ${widget.existing!.fullName}' : 'New driver',
      subtitle: _isEditing
          ? 'Signs in with ${widget.existing!.email}'
          : 'A sign-in is created for the driver and a one-time password is shown once you save.',
      formKey: _formKey,
      isSaving: _isSaving,
      onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
      onSave: _save,
      fields: [
        LabeledField(
          label: 'First name',
          isRequired: true,
          child: AppTextField(
            controller: _firstName,
            validator: (v) => Validators.required(v, 'First name'),
          ),
        ),
        LabeledField(
          label: 'Last name',
          isRequired: true,
          child: AppTextField(
            controller: _lastName,
            validator: (v) => Validators.required(v, 'Last name'),
          ),
        ),
        if (!_isEditing)
          LabeledField(
            label: 'Email',
            isRequired: true,
            child:
                AppTextField(controller: _email, validator: Validators.email),
          ),
        LabeledField(
          label: 'Phone number',
          hint: 'Optional',
          child: AppTextField(
            controller: _phone,
            validator: (v) =>
                v == null || v.trim().isEmpty ? null : Validators.phone(v),
          ),
        ),
        if (!_isEditing)
          LabeledField(
            label: 'Sign-in username',
            isRequired: true,
            child: AppTextField(
              controller: _username,
              hintText: 'For example driver6',
              validator: (v) => Validators.required(v, 'Sign-in username'),
            ),
          ),
        LabeledField(
          label: 'Hire date',
          isRequired: true,
          child: AppDateField(
            value: _hireDate,
            onChanged: (value) => setState(() => _hireDate = value),
          ),
        ),
        LabeledField(
          label: 'Available for work',
          child: AppToggleField(
            value: _isActive,
            onChanged: (value) => setState(() => _isActive = value),
            description: _isActive
                ? 'Receives ride offers while on shift.'
                : 'Kept on the roster but receives no ride offers.',
          ),
        ),
      ],
    );
  }
}
