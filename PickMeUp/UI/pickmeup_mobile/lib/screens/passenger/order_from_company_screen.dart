import 'package:flutter/material.dart';

import '../../models/taxi_company.dart';
import 'order_taxi_screen.dart';

class OrderFromCompanyScreen extends StatelessWidget {
  const OrderFromCompanyScreen({super.key, required this.company});

  final TaxiCompany company;

  static Future<void> open(BuildContext context, TaxiCompany company) {
    return Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => OrderFromCompanyScreen(company: company),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Order from ${company.name ?? ''}')),
      body: OrderTaxiScreen(initialCompany: company),
    );
  }
}
