import 'package:flutter/material.dart';

import '../../layouts/mobile_shell.dart';
import '../../layouts/nav_destination.dart';
import 'companies_screen.dart';
import 'my_reviews_screen.dart';
import '../support/online_support_screen.dart';
import 'order_taxi_screen.dart';
import 'payment_history_screen.dart';
import 'recommended_screen.dart';
import 'ride_status_screen.dart';
import 'settings_screen.dart';
import 'ride_history_screen.dart';
import 'pay_ride_screen.dart';

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
          icon: Icons.navigation_outlined,
          label: 'Ride status',
          builder: (_) => const RideStatusScreen(),
        ),
        MobileDestination(
          icon: Icons.auto_awesome,
          label: 'Recommended for you',
          builder: (_) => const RecommendedScreen(),
        ),
        MobileDestination(
          icon: Icons.local_taxi_outlined,
          label: 'Taxi companies',
          builder: (_) => const CompaniesScreen(),
        ),
        MobileDestination(
          icon: Icons.credit_card,
          label: 'Pay for a ride',
          builder: (_) => const PayRideScreen(),
        ),
        MobileDestination(
          icon: Icons.receipt_long_outlined,
          label: 'Payment history',
          builder: (_) => const PaymentHistoryScreen(),
        ),
        MobileDestination(
          icon: Icons.history,
          label: 'Ride history',
          builder: (_) => const RideHistoryScreen(),
        ),
        MobileDestination(
          icon: Icons.star_outline,
          label: 'My reviews',
          builder: (_) => const MyReviewsScreen(),
        ),
        MobileDestination(
          icon: Icons.settings_outlined,
          label: 'Settings',
          builder: (_) => const SettingsScreen(),
        ),
        MobileDestination(
          icon: Icons.support_agent_outlined,
          label: 'Online support',
          builder: (_) => const OnlineSupportScreen(),
        ),
      ],
    );
  }
}
