import 'package:flutter/material.dart';

import '../layouts/nav_destination.dart';
import '../layouts/role_side_nav.dart';
import 'admin/admin_settings_screen.dart';
import 'admin/pdf_reports_screen.dart';
import 'admin/support_ticket_list_screen.dart';
import 'admin/user_list_screen.dart';
import 'reference/reference_data_hub.dart';
import 'shell_gallery_screen.dart';
import 'taxi_company_list_screen.dart';

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
          builder: (_) => const TaxiCompanyListScreen(),
        ),
        NavDestination(
          label: 'Users',
          icon: Icons.people,
          builder: (_) => const UserListScreen(),
        ),
        NavDestination(
          label: 'Reports & bugs',
          icon: Icons.support_agent,
          builder: (_) => const SupportTicketListScreen(),
        ),
        NavDestination(
          label: 'Reference data',
          icon: Icons.list_alt,
          builder: (_) => const ReferenceDataHub(),
        ),
        NavDestination(
          label: 'PDF reports',
          icon: Icons.picture_as_pdf,
          builder: (_) => const PdfReportsScreen(),
        ),
        NavDestination(
          label: 'Shared shells',
          icon: Icons.widgets_outlined,
          builder: (_) => const ShellGalleryScreen(),
        ),
        NavDestination(
          label: 'Settings',
          icon: Icons.settings,
          builder: (_) => const AdminSettingsScreen(),
        ),
      ],
    );
  }
}
