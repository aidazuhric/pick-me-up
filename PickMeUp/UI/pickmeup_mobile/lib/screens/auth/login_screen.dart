import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_spacing.dart';
import '../../providers/auth_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/validators.dart';
import '../../widgets/auth_header.dart';
import 'sign_up_screen.dart';

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
  bool _obscurePassword = true;
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
    _clearMessages();

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
    _clearMessages();

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
    _clearMessages();

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

  void _clearMessages() {
    setState(() {
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
        setState(
          () => _errorMessage =
              'Could not reach the server. Check that the API is running.',
        );
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
    final challenge = _challenge;

    return Scaffold(
      body: SafeArea(
        top: false,
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              AuthHeader(
                title: 'PickMeUp',
                subtitle: challenge == null
                    ? 'Your ride, a tap away'
                    : 'One more step',
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(
                  AppSpacing.screenPadding,
                  AppSpacing.lg,
                  AppSpacing.screenPadding,
                  AppSpacing.xl,
                ),
                child: challenge == null
                    ? _buildCredentials(theme)
                    : _buildCodeStep(theme, challenge),
              ),
            ],
          ),
        ),
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
          const SizedBox(height: AppSpacing.lg),
          TextFormField(
            controller: _usernameController,
            decoration: const InputDecoration(
              hintText: 'Username',
              prefixIcon: Icon(Icons.person_outline),
            ),
            textInputAction: TextInputAction.next,
            autocorrect: false,
            validator: (value) => Validators.required(value, 'Username'),
          ),
          const SizedBox(height: AppSpacing.md),
          TextFormField(
            controller: _passwordController,
            decoration: InputDecoration(
              hintText: 'Password',
              prefixIcon: const Icon(Icons.lock_outline),
              suffixIcon: IconButton(
                icon: Icon(
                  _obscurePassword
                      ? Icons.visibility_outlined
                      : Icons.visibility_off_outlined,
                ),
                onPressed: () =>
                    setState(() => _obscurePassword = !_obscurePassword),
              ),
            ),
            obscureText: _obscurePassword,
            onFieldSubmitted: (_) => _submit(),
            validator: (value) =>
                (value == null || value.isEmpty) ? 'Password is required.' : null,
          ),
          _buildMessages(theme),
          const SizedBox(height: AppSpacing.lg),
          FilledButton(
            onPressed: _isSubmitting ? null : _submit,
            child: _isSubmitting
                ? const _ButtonSpinner()
                : const Text('SIGN IN'),
          ),
          const SizedBox(height: AppSpacing.md),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text("Don't have an account?", style: theme.textTheme.bodySmall),
              TextButton(
                onPressed: _isSubmitting ? null : _openSignUp,
                child: const Text('Sign up'),
              ),
            ],
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
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  'Check your email',
                  style: theme.textTheme.headlineSmall,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'We sent a sign-in code to ${challenge.maskedEmail}. '
            'It works once, and only for the next ${_minutes(challenge.expiresInSeconds)}.',
            style: theme.textTheme.bodyMedium,
          ),
          const SizedBox(height: AppSpacing.lg),
          TextFormField(
            controller: _codeController,
            autofocus: true,
            decoration: const InputDecoration(
              hintText: 'Code from the email',
              prefixIcon: Icon(Icons.mail_outline),
            ),
            keyboardType: TextInputType.number,
            onFieldSubmitted: (_) => _submitCode(),
            validator: (value) => (value == null || value.trim().isEmpty)
                ? 'Enter the code we sent you.'
                : null,
          ),
          _buildMessages(theme),
          const SizedBox(height: AppSpacing.lg),
          FilledButton(
            onPressed: _isSubmitting ? null : _submitCode,
            child: _isSubmitting ? const _ButtonSpinner() : const Text('VERIFY'),
          ),
          const SizedBox(height: AppSpacing.sm),
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
      padding: const EdgeInsets.only(top: AppSpacing.md),
      child: Text(
        _errorMessage ?? _noticeMessage!,
        style: TextStyle(
          fontSize: 13,
          color: _errorMessage != null
              ? theme.colorScheme.error
              : theme.colorScheme.primary,
        ),
      ),
    );
  }

  Future<void> _openSignUp() async {
    final created = await Navigator.of(context).push<String>(
      MaterialPageRoute<String>(builder: (_) => const SignUpScreen()),
    );

    if (created == null || !mounted) {
      return;
    }

    setState(() {
      _usernameController.text = created;
      _passwordController.clear();
      _noticeMessage = 'Account created. Sign in with your new password.';
    });
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
