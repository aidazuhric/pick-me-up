import 'package:flutter/material.dart';

import '../../layouts/mobile_shell.dart';
import '../../layouts/nav_destination.dart';
import '../../widgets/driver_request_overlay.dart';
import '../passenger/settings_screen.dart';
import '../support/online_support_screen.dart';
import 'active_ride_screen.dart';
import 'completed_rides_screen.dart';
import 'driver_reviews_screen.dart';
import 'report_location_screen.dart';
import 'my_shift_screen.dart';
import 'ride_requests_screen.dart';

class DriverShell extends StatelessWidget {
  const DriverShell({super.key});

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        MobileShell(
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
              icon: Icons.share_location,
              label: 'Report location',
              builder: (_) => const ReportLocationScreen(),
            ),
            MobileDestination(
              icon: Icons.notifications_active_outlined,
              label: 'Ride requests',
              builder: (_) => const RideRequestsScreen(),
            ),
            MobileDestination(
              icon: Icons.done_all,
              label: 'Completed rides',
              builder: (_) => const CompletedRidesScreen(),
            ),
            MobileDestination(
              icon: Icons.star_outline,
              label: 'My reviews',
              builder: (_) => const DriverReviewsScreen(),
            ),
            MobileDestination(
              icon: Icons.support_agent_outlined,
              label: 'Online support',
              builder: (_) => const OnlineSupportScreen(),
            ),
            MobileDestination(
              icon: Icons.settings_outlined,
              label: 'Settings',
              builder: (_) => const SettingsScreen(),
            ),
          ],
        ),
        const DriverRequestOverlay(),
      ],
    );
  }
}
