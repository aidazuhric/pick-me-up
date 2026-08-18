import 'package:flutter/material.dart';

import '../../layouts/mobile_shell.dart';
import '../../layouts/nav_destination.dart';
import '../placeholder_screen.dart';
import 'order_taxi_screen.dart';

class PassengerShell extends StatelessWidget {
  const PassengerShell({super.key});

  @override
  Widget build(BuildContext context) {
    return MobileShell(
      roleLabel: 'Passenger',
      destinations: [
        MobileDestination(
          icon: Icons.home_outlined,
          label: 'Home',
          builder: (_) => const OrderTaxiScreen(),
        ),
        MobileDestination(
          icon: Icons.receipt_long_outlined,
          label: 'Payment history',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.receipt_long_outlined,
            title: 'Payment history',
            description: 'Every card payment you make will be listed here.',
          ),
        ),
        MobileDestination(
          icon: Icons.history,
          label: 'Ride history',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.history,
            title: 'Ride history',
            description:
                'Your past rides, with the driver, the route and the fare.',
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
        MobileDestination(
          icon: Icons.support_agent_outlined,
          label: 'Online support',
          builder: (_) => const PlaceholderScreen(
            icon: Icons.support_agent_outlined,
            title: 'Online support',
            description: 'Ask the PickMeUp team for help and read the answer.',
          ),
        ),
      ],
    );
  }
}
