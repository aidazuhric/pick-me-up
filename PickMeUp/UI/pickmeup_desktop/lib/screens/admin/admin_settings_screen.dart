import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../providers/auth_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';

class AdminSettingsScreen extends StatefulWidget {
  const AdminSettingsScreen({super.key});

  @override
  State<AdminSettingsScreen> createState() => _AdminSettingsScreenState();
}

class _AdminSettingsScreenState extends State<AdminSettingsScreen> {
  bool _notifications = true;
  bool _twoFactor = false;
  bool _isLoadingTwoFactor = true;
  bool _isSavingTwoFactor = false;

  @override
  void initState() {
    super.initState();
    _loadTwoFactor();
  }

  Future<void> _loadTwoFactor() async {
    try {
      final enabled = await context.read<AuthProvider>().readTwoFactorSetting();

      if (!mounted) return;

      setState(() {
        _twoFactor = enabled;
        _isLoadingTwoFactor = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoadingTwoFactor = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  Future<void> _saveTwoFactor(bool enabled) async {
    setState(() => _isSavingTwoFactor = true);

    try {
      final saved = await context.read<AuthProvider>().setTwoFactorSetting(enabled);

      if (!mounted) return;

      setState(() {
        _twoFactor = saved;
        _isSavingTwoFactor = false;
      });

      AppMessages.success(
        context,
        saved
            ? 'From now on we will email you a code each time you sign in.'
            : 'You will sign in with your password only.',
      );
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSavingTwoFactor = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final auth = context.watch<AuthProvider>();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Settings', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'Preferences for the account you are signed in with.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.lg),
        Expanded(
          child: SingleChildScrollView(
            child: Align(
              alignment: Alignment.topLeft,
              child: ConstrainedBox(
                constraints:
                    const BoxConstraints(maxWidth: AppSpacing.formMaxWidth),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _SettingsCard(
                      title: 'Account',
                      children: [
                        _ValueRow(
                          label: 'Signed in as',
                          value: auth.displayName ?? '',
                        ),
                        _ValueRow(label: 'Role', value: auth.role ?? ''),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.md),
                    _SettingsCard(
                      title: 'Preferences',
                      children: [
                        _ToggleRow(
                          label: 'Notifications',
                          description:
                              'Show a message when a ride or a payment changes.',
                          value: _notifications,
                          onChanged: (value) =>
                              setState(() => _notifications = value),
                        ),
                        const _ValueRow(label: 'Language', value: 'English'),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.md),
                    _SettingsCard(
                      title: 'Security',
                      children: [
                        _ToggleRow(
                          label: 'Two-factor authentication',
                          description: _isLoadingTwoFactor
                              ? 'Reading your security settings…'
                              : 'Ask for a code sent to your email every time you sign in.',
                          value: _twoFactor,
                          onChanged:
                              _isLoadingTwoFactor || _isSavingTwoFactor
                                  ? null
                                  : _saveTwoFactor,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}

class _SettingsCard extends StatelessWidget {
  const _SettingsCard({required this.title, required this.children});

  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Text(title, style: theme.textTheme.titleMedium),
          ),
          const Divider(height: 1),
          for (var i = 0; i < children.length; i++) ...[
            if (i > 0) const Divider(height: 1),
            Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: children[i],
            ),
          ],
        ],
      ),
    );
  }
}

class _ToggleRow extends StatelessWidget {
  const _ToggleRow({
    required this.label,
    required this.description,
    required this.value,
    required this.onChanged,
  });

  final String label;
  final String description;
  final bool value;
  final ValueChanged<bool>? onChanged;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label, style: theme.textTheme.bodyMedium),
              const SizedBox(height: 2),
              Text(description, style: theme.textTheme.bodySmall),
            ],
          ),
        ),
        const SizedBox(width: AppSpacing.md),
        Switch(value: value, onChanged: onChanged),
      ],
    );
  }
}

class _ValueRow extends StatelessWidget {
  const _ValueRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        Expanded(child: Text(label, style: theme.textTheme.bodyMedium)),
        Text(value, style: theme.textTheme.bodySmall),
      ],
    );
  }
}
