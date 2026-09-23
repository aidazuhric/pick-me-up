import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/constants/roles.dart';
import 'core/theme/app_theme.dart';
import 'providers/auth_provider.dart';
import 'providers/user_provider.dart';
import 'screens/admin_shell.dart';
import 'screens/company_shell.dart';
import 'screens/force_password_change_screen.dart';
import 'screens/login_screen.dart';
import 'screens/unsupported_role_screen.dart';

void main() {
  runApp(const PickMeUpDesktopApp());
}

class PickMeUpDesktopApp extends StatelessWidget {
  const PickMeUpDesktopApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => UserProvider()),
      ],
      child: MaterialApp(
        title: 'PickMeUp',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.build(),
        home: const RootRouter(),
      ),
    );
  }
}

class RootRouter extends StatelessWidget {
  const RootRouter({super.key});

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    if (!auth.isAuthenticated) {
      return const LoginScreen();
    }

    if (auth.mustChangePassword) {
      return const ForcePasswordChangeScreen();
    }

    switch (auth.role) {
      case Roles.admin:
        return const AdminShell();
      case Roles.company:
        return const CompanyShell();
      default:
        return UnsupportedRoleScreen(role: auth.role);
    }
  }
}
