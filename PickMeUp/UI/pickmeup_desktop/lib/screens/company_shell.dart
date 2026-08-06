import 'package:flutter/material.dart';

import '../layouts/nav_destination.dart';
import '../layouts/role_side_nav.dart';
import 'company/car_assignment_list_screen.dart';
import 'company/car_list_screen.dart';
import 'company/company_profile_screen.dart';
import 'company/driver_list_screen.dart';
import 'placeholder_screen.dart';

class CompanyShell extends StatelessWidget {
  const CompanyShell({super.key});

  @override
  Widget build(BuildContext context) {
    return RoleSideNav(
      roleLabel: 'Taxi company',
      destinations: [
        NavDestination(
          label: 'Company profile',
          icon: Icons.badge,
          builder: (_) => const CompanyProfileScreen(),
        ),
        NavDestination(
          label: 'Drivers',
          icon: Icons.person_pin,
          builder: (_) => const DriverListScreen(),
        ),
        NavDestination(
          label: 'Vehicles',
          icon: Icons.directions_car,
          builder: (_) => const CarListScreen(),
        ),
        NavDestination(
          label: 'Assignments',
          icon: Icons.link,
          builder: (_) => const CarAssignmentListScreen(),
        ),
        NavDestination(
          label: 'Orders',
          icon: Icons.receipt_long,
          builder: (_) => const PlaceholderScreen(
            title: 'Orders',
            arrivesWith: 'the Order entity',
          ),
        ),
        NavDestination(
          label: 'Shifts',
          icon: Icons.schedule,
          builder: (_) => const PlaceholderScreen(
            title: 'Driver shifts',
            arrivesWith: 'the Shift entity',
          ),
        ),
        NavDestination(
          label: 'Reviews',
          icon: Icons.star_rate,
          builder: (_) => const PlaceholderScreen(
            title: 'Reviews',
            arrivesWith: 'the Review entity',
          ),
        ),
        NavDestination(
          label: 'Fleet map',
          icon: Icons.map,
          builder: (_) => const PlaceholderScreen(
            title: 'Fleet tracking',
            arrivesWith: 'live vehicle positions',
          ),
        ),
        NavDestination(
          label: 'Payments',
          icon: Icons.payments,
          builder: (_) => const PlaceholderScreen(
            title: 'Payment status & history',
            arrivesWith: 'the Payment entity',
          ),
        ),
        NavDestination(
          label: 'Business report',
          icon: Icons.picture_as_pdf,
          builder: (_) => const PlaceholderScreen(
            title: 'Business report',
            arrivesWith: 'the reporting endpoints',
          ),
        ),
        NavDestination(
          label: 'Online support',
          icon: Icons.support_agent,
          builder: (_) => const PlaceholderScreen(
            title: 'Online support',
            arrivesWith: 'the SupportTicket entity',
          ),
        ),
      ],
    );
  }
}
