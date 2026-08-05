import 'package:flutter/material.dart';

import '../layouts/nav_destination.dart';
import '../layouts/role_side_nav.dart';
import 'placeholder_screen.dart';
import 'shell_gallery_screen.dart';

class AdminShell extends StatelessWidget {
  const AdminShell({super.key});

  @override
  Widget build(BuildContext context) {
    return RoleSideNav(
      roleLabel: 'Admin',
      destinations: [
        NavDestination(
          label: 'Taxi companies',
          icon: Icons.business,
          builder: (_) => const PlaceholderScreen(
            title: 'Taxi companies',
            arrivesWith: 'the TaxiCompany entity',
          ),
        ),
        NavDestination(
          label: 'Users',
          icon: Icons.people,
          builder: (_) => const PlaceholderScreen(
            title: 'Users',
            arrivesWith: 'the user management endpoints',
          ),
        ),
        NavDestination(
          label: 'Reports & bugs',
          icon: Icons.support_agent,
          builder: (_) => const PlaceholderScreen(
            title: 'Reports & bugs',
            arrivesWith: 'the SupportTicket entity',
          ),
        ),
        NavDestination(
          label: 'Reference data',
          icon: Icons.list_alt,
          builder: (_) => const PlaceholderScreen(
            title: 'Reference data',
            arrivesWith: 'the reference tables',
          ),
        ),
        NavDestination(
          label: 'PDF reports',
          icon: Icons.picture_as_pdf,
          builder: (_) => const PlaceholderScreen(
            title: 'PDF reports',
            arrivesWith: 'the reporting endpoints',
          ),
        ),
        NavDestination(
          label: 'Shared shells',
          icon: Icons.widgets_outlined,
          builder: (_) => const ShellGalleryScreen(),
        ),
        NavDestination(
          label: 'Settings',
          icon: Icons.settings,
          builder: (_) => const PlaceholderScreen(
            title: 'Settings',
            arrivesWith: 'two-factor authentication',
          ),
        ),
      ],
    );
  }
}
