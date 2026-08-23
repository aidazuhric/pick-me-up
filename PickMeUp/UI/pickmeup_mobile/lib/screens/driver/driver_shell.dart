import 'package:flutter/material.dart';

import '../../layouts/mobile_shell.dart';
import '../../layouts/nav_destination.dart';
import '../placeholder_screen.dart';
import 'active_ride_screen.dart';
import 'my_shift_screen.dart';
import 'ride_requests_screen.dart';

class DriverShell extends StatelessWidget {
  const DriverShell({super.key});

  @override
  Widget build(BuildContext context) {
    return MobileShell(
      roleLabel: 'Driver',
      destinations: [
        MobileDestination(
          icon: Icons.home_outlined,
          label: 'Active ride',
          builder: (_) => const ActiveRideScreen(),
        ),
        MobileDestination(
          icon: Icons.schedule_outlined,
          label: 'My shift',
          builder: (_) => const MyShiftScreen(),
        ),
        MobileDestination(
          icon: Icons.notifications_active_outlined,
          label: 'Ride requests',
          builder: (_) => const RideRequestsScreen(),
        ),
        MobileDestination(
          icon: Icons.done_all,
          label: 'Completed rides',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.done_all,
            title: 'Completed rides',
            description: 'What you drove, and what it earned.',
          ),
        ),
        MobileDestination(
          icon: Icons.star_outline,
          label: 'My reviews',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.star_outline,
            title: 'My reviews',
            description: 'Ratings and comments your passengers left.',
          ),
        ),
        MobileDestination(
          icon: Icons.settings_outlined,
          label: 'Settings',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.settings_outlined,
            title: 'Settings',
            description:
                'Profile details, notifications and two-factor sign-in.',
          ),
        ),
      ],
    );
  }
}
