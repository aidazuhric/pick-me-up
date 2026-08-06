import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../models/user.dart';
import '../../providers/user_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/labeled_form.dart';

class UserForm extends StatefulWidget {
  const UserForm({
    super.key,
    required this.user,
    required this.provider,
    required this.isSelf,
  });

  final User user;
  final UserProvider provider;
  final bool isSelf;

  static Future<bool> show(
    BuildContext context, {
    required User user,
    required UserProvider provider,
    required bool isSelf,
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
            height: isSelf ? 660 : 520,
            child: UserForm(user: user, provider: provider, isSelf: isSelf),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<UserForm> createState() => _UserFormState();
}

class _UserFormState extends State<UserForm> {
  final _formKey = GlobalKey<FormState>();
  final _firstName = TextEditingController();
  final _lastName = TextEditingController();
  final _email = TextEditingController();
  final _username = TextEditingController();
  final _phone = TextEditingController();
  final _currentPassword = TextEditingController();
  final _newPassword = TextEditingController();
  final _confirmPassword = TextEditingController();

  bool _isActive = true;
  bool _isChangingPassword = false;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();

    _firstName.text = widget.user.firstName ?? '';
    _lastName.text = widget.user.lastName ?? '';
    _email.text = widget.user.email ?? '';
    _username.text = widget.user.username ?? '';
    _phone.text = widget.user.phoneNumber ?? '';
    _isActive = widget.user.isActive ?? true;
  }

  @override
  void dispose() {
    for (final controller in [
      _firstName,
      _lastName,
      _email,
      _username,
      _phone,
      _currentPassword,
      _newPassword,
      _confirmPassword,
    ]) {
      controller.dispose();
    }
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final saved = await widget.provider.update(widget.user.id!, {
        'firstName': _firstName.text.trim(),
        'lastName': _lastName.text.trim(),
        'email': _email.text.trim(),
        'username': _username.text.trim(),
        'phoneNumber':
            _phone.text.trim().isEmpty ? null : _phone.text.trim(),
        'isActive': _isActive,
      });

      if (_isChangingPassword) {
        await widget.provider.changePassword(
          id: widget.user.id!,
          currentPassword: _currentPassword.text,
          newPassword: _newPassword.text,
          confirmPassword: _confirmPassword.text,
        );
      }

      if (!mounted) return;

      AppMessages.success(
        context,
        _isChangingPassword
            ? '${saved.fullName} was saved and the password was changed.'
            : '${saved.fullName} was saved.',
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
      title: 'Edit ${widget.user.fullName}',
      subtitle: widget.isSelf
          ? 'This is your own account.'
          : 'Signs in as ${widget.user.username} · ${widget.user.role}',
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
            validator: (v) =>
                Validators.required(v, 'First name') ??
                Validators.maxLength(v, 50, 'First name'),
          ),
        ),
        LabeledField(
          label: 'Last name',
          isRequired: true,
          child: AppTextField(
            controller: _lastName,
            validator: (v) =>
                Validators.required(v, 'Last name') ??
                Validators.maxLength(v, 50, 'Last name'),
          ),
        ),
        LabeledField(
          label: 'Email',
          isRequired: true,
          child: AppTextField(controller: _email, validator: Validators.email),
        ),
        LabeledField(
          label: 'Username',
          isRequired: true,
          child: AppTextField(
            controller: _username,
            validator: (v) => Validators.required(v, 'Username'),
          ),
        ),
        LabeledField(
          label: 'Phone number',
          hint: 'Optional',
          child: AppTextField(
            controller: _phone,
            validator: (v) => v == null || v.trim().isEmpty
                ? null
                : Validators.phone(v),
          ),
        ),
        LabeledField(
          label: 'Account active',
          child: AppToggleField(
            value: _isActive,
            onChanged: (value) => setState(() => _isActive = value),
            description: _isActive
                ? 'The account can sign in.'
                : 'The account is suspended and cannot sign in.',
          ),
        ),
        if (widget.isSelf) ...[
          LabeledField(
            label: 'Password',
            child: AppToggleField(
              value: _isChangingPassword,
              onChanged: (value) => setState(() => _isChangingPassword = value),
              description: 'Change my password',
            ),
          ),
          if (_isChangingPassword) ...[
            LabeledField(
              label: 'Current password',
              isRequired: true,
              child: AppTextField(
                controller: _currentPassword,
                obscureText: true,
                validator: (v) => Validators.required(v, 'Current password'),
              ),
            ),
            LabeledField(
              label: 'New password',
              isRequired: true,
              child: AppTextField(
                controller: _newPassword,
                obscureText: true,
                validator: (v) =>
                    Validators.required(v, 'New password') ??
                    (v!.trim().length < 6
                        ? 'New password must be at least 6 characters.'
                        : null),
              ),
            ),
            LabeledField(
              label: 'Confirm new password',
              isRequired: true,
              child: AppTextField(
                controller: _confirmPassword,
                obscureText: true,
                validator: (v) => v == _newPassword.text
                    ? null
                    : 'The two passwords do not match.',
              ),
            ),
          ],
        ],
      ],
    );
  }
}
