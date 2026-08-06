import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../providers/auth_provider.dart';
import '../utils/api_client_exception.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _codeFormKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _codeController = TextEditingController();

  bool _isSubmitting = false;
  String? _errorMessage;
  String? _noticeMessage;
  TwoFactorChallenge? _challenge;

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    _codeController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _errorMessage = null;
      _noticeMessage = null;
    });

    if (!_formKey.currentState!.validate()) {
      return;
    }

    await _run(() async {
      final challenge = await context.read<AuthProvider>().login(
            _usernameController.text.trim(),
            _passwordController.text,
          );

      if (challenge != null && mounted) {
        setState(() {
          _challenge = challenge;
          _codeController.clear();
        });
      }
    });
  }

  Future<void> _submitCode() async {
    setState(() {
      _errorMessage = null;
      _noticeMessage = null;
    });

    if (!_codeFormKey.currentState!.validate()) {
      return;
    }

    await _run(() async {
      await context.read<AuthProvider>().verifyCode(
            _usernameController.text.trim(),
            _codeController.text.trim(),
          );
    });
  }

  Future<void> _resendCode() async {
    setState(() {
      _errorMessage = null;
      _noticeMessage = null;
    });

    await _run(() async {
      final challenge = await context.read<AuthProvider>().login(
            _usernameController.text.trim(),
            _passwordController.text,
          );

      if (!mounted) return;

      setState(() {
        _challenge = challenge ?? _challenge;
        _noticeMessage = 'We sent another code. Check your inbox.';
      });
    });
  }

  void _backToCredentials() {
    setState(() {
      _challenge = null;
      _codeController.clear();
      _errorMessage = null;
      _noticeMessage = null;
    });
  }

  Future<void> _run(Future<void> Function() action) async {
    setState(() => _isSubmitting = true);

    try {
      await action();
    } on ApiClientException catch (e) {
      if (mounted) {
        setState(() => _errorMessage = e.message);
      }
    } catch (_) {
      if (mounted) {
        setState(() => _errorMessage =
            'Could not reach the server. Check that the API is running.');
      }
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: Row(
        children: [
          Expanded(
            flex: 2,
            child: Container(
              color: AppColors.primaryIndigo,
              child: const Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.local_taxi, size: 96, color: Colors.white),
                    SizedBox(height: 20),
                    Text(
                      'PickMeUp',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 34,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    SizedBox(height: 8),
                    Text(
                      'Administration and company portal',
                      style: TextStyle(color: Colors.white70, fontSize: 15),
                    ),
                  ],
                ),
              ),
            ),
          ),
          Expanded(
            flex: 3,
            child: Center(
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 420),
                child: SingleChildScrollView(
                  padding: const EdgeInsets.all(40),
                  child: _challenge == null
                      ? _buildCredentials(theme)
                      : _buildCodeStep(theme, _challenge!),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCredentials(ThemeData theme) {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text('Sign in', style: theme.textTheme.headlineMedium),
          const SizedBox(height: 28),
          TextFormField(
            controller: _usernameController,
            decoration: const InputDecoration(labelText: 'Username'),
            textInputAction: TextInputAction.next,
            validator: (value) => (value == null || value.trim().isEmpty)
                ? 'Enter your username.'
                : null,
          ),
          const SizedBox(height: 20),
          TextFormField(
            controller: _passwordController,
            decoration: const InputDecoration(labelText: 'Password'),
            obscureText: true,
            onFieldSubmitted: (_) => _submit(),
            validator: (value) =>
                (value == null || value.isEmpty) ? 'Enter your password.' : null,
          ),
          _buildMessages(theme),
          const SizedBox(height: 32),
          FilledButton(
            onPressed: _isSubmitting ? null : _submit,
            child: _isSubmitting ? const _ButtonSpinner() : const Text('SIGN IN'),
          ),
        ],
      ),
    );
  }

  Widget _buildCodeStep(ThemeData theme, TwoFactorChallenge challenge) {
    return Form(
      key: _codeFormKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              IconButton(
                tooltip: 'Back',
                onPressed: _isSubmitting ? null : _backToCredentials,
                icon: const Icon(Icons.arrow_back),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text('Check your email',
                    style: theme.textTheme.headlineSmall),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            'We sent a sign-in code to ${challenge.maskedEmail}. '
            'It works once, and only for the next ${_minutes(challenge.expiresInSeconds)}.',
            style: theme.textTheme.bodyMedium,
          ),
          const SizedBox(height: 28),
          TextFormField(
            controller: _codeController,
            autofocus: true,
            decoration: const InputDecoration(labelText: 'Code from the email'),
            keyboardType: TextInputType.number,
            onFieldSubmitted: (_) => _submitCode(),
            validator: (value) => (value == null || value.trim().isEmpty)
                ? 'Enter the code we sent you.'
                : null,
          ),
          _buildMessages(theme),
          const SizedBox(height: 32),
          FilledButton(
            onPressed: _isSubmitting ? null : _submitCode,
            child: _isSubmitting ? const _ButtonSpinner() : const Text('VERIFY'),
          ),
          const SizedBox(height: 12),
          TextButton(
            onPressed: _isSubmitting ? null : _resendCode,
            child: const Text('Send a new code'),
          ),
        ],
      ),
    );
  }

  Widget _buildMessages(ThemeData theme) {
    if (_errorMessage == null && _noticeMessage == null) {
      return const SizedBox.shrink();
    }

    return Padding(
      padding: const EdgeInsets.only(top: 16),
      child: Text(
        _errorMessage ?? _noticeMessage!,
        style: TextStyle(
          color: _errorMessage != null
              ? theme.colorScheme.error
              : theme.colorScheme.primary,
        ),
      ),
    );
  }

  static String _minutes(int seconds) {
    final minutes = (seconds / 60).ceil();
    return minutes <= 1 ? 'minute' : '$minutes minutes';
  }
}

class _ButtonSpinner extends StatelessWidget {
  const _ButtonSpinner();

  @override
  Widget build(BuildContext context) {
    return const SizedBox(
      height: 20,
      width: 20,
      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
    );
  }
}
