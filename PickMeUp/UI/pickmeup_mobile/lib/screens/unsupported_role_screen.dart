import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_spacing.dart';
import '../providers/auth_provider.dart';

class UnsupportedRoleScreen extends StatelessWidget {
  const UnsupportedRoleScreen({super.key, required this.role});

  final String? role;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final roleName = role ?? 'This account';

    return Scaffold(
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.desktop_windows_outlined, size: 64),
              const SizedBox(height: AppSpacing.lg),
              Text('Use the desktop app', style: theme.textTheme.headlineSmall),
              const SizedBox(height: AppSpacing.sm),
              Text(
                '$roleName has no mobile screens. The mobile client serves the '
                'Passenger and Driver roles.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium,
              ),
              const SizedBox(height: AppSpacing.lg),
              FilledButton(
                onPressed: () => context.read<AuthProvider>().logout(),
                child: const Text('SIGN OUT'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
