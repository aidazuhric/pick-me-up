import 'package:flutter/material.dart';
import 'package:flutter_stripe/flutter_stripe.dart';

import '../models/payment.dart';
import '../providers/payment_provider.dart';
import '../utils/api_client_exception.dart';

enum RidePaymentOutcome { paid, cancelled, declined, unconfirmed, failed }

class RidePaymentResult {
  const RidePaymentResult(this.outcome, this.message);

  final RidePaymentOutcome outcome;
  final String message;

  bool get isPaid => outcome == RidePaymentOutcome.paid;
}

class RidePaymentFlow {
  RidePaymentFlow._();

  static const Duration pollInterval = Duration(milliseconds: 1500);
  static const int pollAttempts = 20;

  static const String gatewayOffNotice =
      'Card payments are not switched on yet. Ask an administrator to add the '
      'Stripe publishable key.';

  static Future<String?> prepareGateway(PaymentProvider provider) async {
    final config = await provider.readConfig();
    final key = config.publishableKey ?? '';

    if (key.isEmpty) {
      return gatewayOffNotice;
    }

    Stripe.publishableKey = key;
    await Stripe.instance.applySettings();

    return null;
  }

  static Future<RidePaymentResult> pay(
    PaymentProvider provider,
    int orderId,
  ) async {
    PaymentIntentDetails intent;

    try {
      intent = await provider.createIntent(orderId);
    } on ApiClientException catch (e) {
      return RidePaymentResult(RidePaymentOutcome.failed, e.message);
    } catch (_) {
      return const RidePaymentResult(
        RidePaymentOutcome.failed,
        'Could not start the payment. Please try again.',
      );
    }

    try {
      await Stripe.instance.initPaymentSheet(
        paymentSheetParameters: SetupPaymentSheetParameters(
          paymentIntentClientSecret: intent.clientSecret ?? '',
          merchantDisplayName: 'PickMeUp',
          style: ThemeMode.light,
        ),
      );

      await Stripe.instance.presentPaymentSheet();
    } on StripeException catch (e) {
      if (e.error.code == FailureCode.Canceled) {
        return const RidePaymentResult(
          RidePaymentOutcome.cancelled,
          'Payment cancelled. The ride has not been sent to drivers yet.',
        );
      }

      return RidePaymentResult(
        RidePaymentOutcome.declined,
        e.error.localizedMessage ??
            e.error.message ??
            'That card was declined. Try another card.',
      );
    } catch (_) {
      return const RidePaymentResult(
        RidePaymentOutcome.failed,
        'The payment sheet could not be opened. Please try again.',
      );
    }

    return await awaitConfirmation(provider, orderId);
  }

  static Future<RidePaymentResult> awaitConfirmation(
    PaymentProvider provider,
    int orderId,
  ) async {
    for (var attempt = 0; attempt < pollAttempts; attempt++) {
      await Future<void>.delayed(pollInterval);

      try {
        final payment = await provider.readByOrder(orderId);

        if (payment.isPaid == true) {
          return const RidePaymentResult(
            RidePaymentOutcome.paid,
            'Paid — waiting for a driver to accept.',
          );
        }
      } on ApiClientException catch (e) {
        if (e.isUnauthorized) {
          return RidePaymentResult(RidePaymentOutcome.failed, e.message);
        }
      } catch (_) {
        continue;
      }
    }

    return const RidePaymentResult(
      RidePaymentOutcome.unconfirmed,
      'Your card went through, but the confirmation has not arrived yet. '
      'Pull down to refresh in a moment.',
    );
  }
}
