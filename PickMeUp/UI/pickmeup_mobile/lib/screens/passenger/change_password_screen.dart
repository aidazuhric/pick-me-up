import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../providers/user_provider.dart';
import '../../utils/api_client_exception.dart';

class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});

  static const int minLength = 6;

  static Future<bool?> open(BuildContext context) {
    return Navigator.of(context).push<bool>(
      MaterialPageRoute<bool>(builder: (_) => const ChangePasswordScreen()),
    );
  }

  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  final _currentController = TextEditingController();
  final _newController = TextEditingController();
  final _confirmController = TextEditingController();
  final _users = UserProvider();

  bool _isSubmitting = false;
  bool _obscure = true;
  String? _submitError;

  @override
  void dispose() {
    _currentController.dispose();
    _newController.dispose();
    _confirmController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _submitError = null);

    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSubmitting = true);

    final navigator = Navigator.of(context);

    try {
      await _users.changePassword(
        currentPassword: _currentController.text,
        newPassword: _newController.text,
        confirmPassword: _confirmController.text,
      );

      navigator.pop(true);
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _submitError = e.message;
        _isSubmitting = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _submitError =
            'Could not reach the server. Check that the API is running.';
        _isSubmitting = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Change password')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(AppSpacing.screenPadding),
          children: [
            Text(
              'Enter the password you use now, then the new one twice.',
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.lg),
            TextFormField(
              autovalidateMode: AutovalidateMode.onUserInteraction,
              controller: _currentController,
              enabled: !_isSubmitting,
              obscureText: _obscure,
              decoration: InputDecoration(
                hintText: 'Current password',
                prefixIcon: const Icon(Icons.lock_outline),
                suffixIcon: IconButton(
                  icon: Icon(
                    _obscure
                        ? Icons.visibility_outlined
                        : Icons.visibility_off_outlined,
                  ),
                  onPressed: () => setState(() => _obscure = !_obscure),
                ),
              ),
              validator: (value) => (value == null || value.isEmpty)
                  ? 'Enter your current password.'
                  : null,
            ),
            const SizedBox(height: AppSpacing.md),
            TextFormField(
              autovalidateMode: AutovalidateMode.onUserInteraction,
              controller: _newController,
              enabled: !_isSubmitting,
              obscureText: _obscure,
              decoration: const InputDecoration(
                hintText: 'New password',
                prefixIcon: Icon(Icons.lock_reset),
              ),
              validator: (value) {
                final text = value ?? '';

                if (text.isEmpty) {
                  return 'Enter a new password.';
                }
                if (text.length < ChangePasswordScreen.minLength) {
                  return 'New password must be at least '
                      '${ChangePasswordScreen.minLength} characters.';
                }
                return null;
              },
            ),
            const SizedBox(height: AppSpacing.md),
            TextFormField(
              autovalidateMode: AutovalidateMode.onUserInteraction,
              controller: _confirmController,
              enabled: !_isSubmitting,
              obscureText: _obscure,
              decoration: const InputDecoration(
                hintText: 'Repeat the new password',
                prefixIcon: Icon(Icons.lock_reset),
              ),
              validator: (value) => value != _newController.text
                  ? 'Passwords do not match.'
                  : null,
            ),
            if (_submitError != null)
              Padding(
                padding: const EdgeInsets.only(top: AppSpacing.md),
                child: Text(
                  _submitError!,
                  style: TextStyle(
                    color: theme.colorScheme.error,
                    fontSize: 13,
                  ),
                ),
              ),
            const SizedBox(height: AppSpacing.lg),
            FilledButton(
              onPressed: _isSubmitting ? null : _submit,
              child: _isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Text('CHANGE PASSWORD'),
            ),
          ],
        ),
      ),
    );
  }
}
