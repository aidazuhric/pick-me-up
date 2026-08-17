import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import 'core/constants/roles.dart';
import 'core/theme/app_theme.dart';
import 'providers/auth_provider.dart';
import 'providers/user_provider.dart';
import 'screens/auth/login_screen.dart';
import 'screens/driver/driver_shell.dart';
import 'screens/passenger/passenger_shell.dart';
import 'screens/unsupported_role_screen.dart';

void main() {
  runApp(const PickMeUpMobileApp());
}

class PickMeUpMobileApp extends StatelessWidget {
  const PickMeUpMobileApp({super.key});

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

    switch (auth.role) {
      case Roles.passenger:
        return const PassengerShell();
      case Roles.driver:
        return const DriverShell();
      default:
        return UnsupportedRoleScreen(role: auth.role);
    }
  }
}
