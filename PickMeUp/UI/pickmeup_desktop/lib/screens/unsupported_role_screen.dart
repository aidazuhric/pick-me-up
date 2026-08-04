import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

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
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 460),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.phone_iphone, size: 64),
              const SizedBox(height: 20),
              Text('Use the mobile app', style: theme.textTheme.headlineSmall),
              const SizedBox(height: 12),
              Text(
                '$roleName has no desktop portal. The desktop client serves the '
                'Admin and Taxi company roles.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium,
              ),
              const SizedBox(height: 28),
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
