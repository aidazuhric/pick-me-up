import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../providers/auth_provider.dart';
import '../providers/user_provider.dart';
import '../utils/api_client_exception.dart';

class ForcePasswordChangeScreen extends StatefulWidget {
  const ForcePasswordChangeScreen({super.key});

  static const int minLength = 6;

  @override
  State<ForcePasswordChangeScreen> createState() =>
      _ForcePasswordChangeScreenState();
}

class _ForcePasswordChangeScreenState extends State<ForcePasswordChangeScreen> {
  final _formKey = GlobalKey<FormState>();
  final _currentController = TextEditingController();
  final _newController = TextEditingController();
  final _confirmController = TextEditingController();
  final _users = UserProvider();

  bool _isSubmitting = false;
  String? _errorMessage;

  @override
  void dispose() {
    _currentController.dispose();
    _newController.dispose();
    _confirmController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _errorMessage = null);

    if (!_formKey.currentState!.validate()) {
      return;
    }

    final auth = context.read<AuthProvider>();
    final userId = auth.userId;

    if (userId == null) {
      setState(() => _errorMessage = 'Your session has expired. Sign in again.');
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      await _users.changePassword(
        id: userId,
        currentPassword: _currentController.text,
        newPassword: _newController.text,
        confirmPassword: _confirmController.text,
      );

      auth.passwordChanged();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
        _errorMessage = e.message;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _isSubmitting = false;
        _errorMessage = 'Could not change your password. Please try again.';
      });
    }
  }

  String? _newPasswordError(String? value) {
    final text = value ?? '';

    if (text.isEmpty) {
      return 'A new password is required.';
    }
    if (text.length < ForcePasswordChangeScreen.minLength) {
      return 'The new password must be at least '
          '${ForcePasswordChangeScreen.minLength} characters.';
    }
    if (text == _currentController.text) {
      return 'The new password must be different from the current one.';
    }
    return null;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final auth = context.watch<AuthProvider>();

    return Scaffold(
      backgroundColor: AppColors.sectionTint,
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 460),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text('Set a new password', style: theme.textTheme.titleLarge),
                      const SizedBox(height: AppSpacing.sm),
                      Text(
                        auth.displayName == null
                            ? 'Your account was created with a password someone else chose. Pick your own before you continue.'
                            : '${auth.displayName}, your account was created with a password someone else chose. Pick your own before you continue.',
                        style: theme.textTheme.bodyMedium,
                      ),
                      const SizedBox(height: AppSpacing.lg),
                      TextFormField(
                        controller: _currentController,
                        enabled: !_isSubmitting,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'Current password',
                        ),
                        autovalidateMode: AutovalidateMode.onUserInteraction,
                        validator: (v) => (v ?? '').isEmpty
                            ? 'Your current password is required.'
                            : null,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        controller: _newController,
                        enabled: !_isSubmitting,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'New password',
                        ),
                        autovalidateMode: AutovalidateMode.onUserInteraction,
                        validator: _newPasswordError,
                      ),
                      const SizedBox(height: AppSpacing.md),
                      TextFormField(
                        controller: _confirmController,
                        enabled: !_isSubmitting,
                        obscureText: true,
                        decoration: const InputDecoration(
                          labelText: 'Confirm new password',
                        ),
                        autovalidateMode: AutovalidateMode.onUserInteraction,
                        validator: (v) => v != _newController.text
                            ? 'The two passwords do not match.'
                            : null,
                      ),
                      if (_errorMessage != null) ...[
                        const SizedBox(height: AppSpacing.md),
                        Text(
                          _errorMessage!,
                          style: theme.textTheme.bodySmall
                              ?.copyWith(color: theme.colorScheme.error),
                        ),
                      ],
                      const SizedBox(height: AppSpacing.lg),
                      FilledButton(
                        onPressed: _isSubmitting ? null : _submit,
                        child: Text(_isSubmitting ? 'SAVING…' : 'SAVE AND CONTINUE'),
                      ),
                      const SizedBox(height: AppSpacing.sm),
                      TextButton(
                        onPressed: _isSubmitting
                            ? null
                            : () => context.read<AuthProvider>().logout(),
                        child: const Text('Sign out'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
