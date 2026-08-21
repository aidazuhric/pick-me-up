import 'package:flutter/material.dart';
import 'package:flutter_stripe/flutter_stripe.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/payable_order.dart';
import '../../models/payment.dart';
import '../../providers/payment_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';

enum _RideStage { unpaid, paying, confirming, paid }

class _RideCard {
  _RideCard({required this.ride});

  final PayableOrder ride;
  _RideStage stage = _RideStage.unpaid;
  Payment? payment;
}

class PayRideScreen extends StatefulWidget {
  const PayRideScreen({super.key});

  @override
  State<PayRideScreen> createState() => _PayRideScreenState();
}

class _PayRideScreenState extends State<PayRideScreen> {
  static const Duration _pollInterval = Duration(milliseconds: 1500);
  static const int _pollAttempts = 20;

  final _provider = PaymentProvider();

  List<_RideCard> _cards = const [];
  bool _isLoading = true;
  String? _error;
  String? _gatewayNotice;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final config = await _provider.readConfig();
      final key = config.publishableKey ?? '';

      if (key.isEmpty) {
        setState(() {
          _gatewayNotice =
              'Card payments are not switched on yet. Ask an administrator to '
              'add the Stripe publishable key.';
        });
      } else {
        Stripe.publishableKey = key;
        await Stripe.instance.applySettings();
        setState(() => _gatewayNotice = null);
      }

      final rides = await _provider.readPayable();

      if (!mounted) return;

      final paid = _cards.where((c) => c.stage == _RideStage.paid).toList();
      final paidOrderIds = paid.map((c) => c.ride.orderId).toSet();

      setState(() {
        _cards = [
          ...paid,
          ...rides
              .where((r) => !paidOrderIds.contains(r.orderId))
              .map((r) => _RideCard(ride: r)),
        ];
        _isLoading = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  Future<void> _pay(_RideCard card) async {
    final orderId = card.ride.orderId;

    if (orderId == null || _gatewayNotice != null) {
      return;
    }

    setState(() => card.stage = _RideStage.paying);

    PaymentIntentDetails intent;

    try {
      intent = await _provider.createIntent(orderId);
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => card.stage = _RideStage.unpaid);
      AppMessages.failure(context, e.message);
      return;
    } catch (_) {
      if (!mounted) return;
      setState(() => card.stage = _RideStage.unpaid);
      AppMessages.failure(
        context,
        'Could not start the payment. Please try again.',
      );
      return;
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
      if (!mounted) return;

      setState(() => card.stage = _RideStage.unpaid);

      final isCancelled = e.error.code == FailureCode.Canceled;

      AppMessages.failure(
        context,
        isCancelled
            ? 'Payment cancelled. The ride is still unpaid.'
            : e.error.localizedMessage ??
                  e.error.message ??
                  'That card was declined. Try another card.',
      );
      return;
    } catch (_) {
      if (!mounted) return;
      setState(() => card.stage = _RideStage.unpaid);
      AppMessages.failure(
        context,
        'The payment sheet could not be opened. Please try again.',
      );
      return;
    }

    if (!mounted) return;

    setState(() => card.stage = _RideStage.confirming);

    await _awaitConfirmation(card, orderId);
  }

  Future<void> _awaitConfirmation(_RideCard card, int orderId) async {
    for (var attempt = 0; attempt < _pollAttempts; attempt++) {
      await Future<void>.delayed(_pollInterval);

      if (!mounted) return;

      try {
        final payment = await _provider.readByOrder(orderId);

        if (payment.isPaid == true) {
          if (!mounted) return;

          setState(() {
            card.stage = _RideStage.paid;
            card.payment = payment;
          });

          AppMessages.success(context, 'Ride #$orderId is paid. Thank you!');
          return;
        }
      } on ApiClientException catch (e) {
        if (!e.isUnauthorized) {
          continue;
        }

        if (!mounted) return;

        setState(() => card.stage = _RideStage.unpaid);
        AppMessages.failure(context, e.message);
        return;
      } catch (_) {
        continue;
      }
    }

    if (!mounted) return;

    setState(() => card.stage = _RideStage.unpaid);

    AppMessages.failure(
      context,
      'Your card went through, but the confirmation has not arrived yet. '
      'Pull down to refresh in a moment.',
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          if (_gatewayNotice != null) _GatewayNotice(message: _gatewayNotice!),
          if (_error != null) _ErrorNotice(message: _error!),
          if (_error == null && _cards.isEmpty) const _EmptyState(),
          for (final card in _cards)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.md),
              child: _RidePaymentCard(
                card: card,
                onPay: () => _pay(card),
                isBlocked: _gatewayNotice != null,
              ),
            ),
        ],
      ),
    );
  }
}

class _RidePaymentCard extends StatelessWidget {
  const _RidePaymentCard({
    required this.card,
    required this.onPay,
    required this.isBlocked,
  });

  final _RideCard card;
  final VoidCallback onPay;
  final bool isBlocked;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final ride = card.ride;
    final isPaid = card.stage == _RideStage.paid;
    final isBusy =
        card.stage == _RideStage.paying || card.stage == _RideStage.confirming;

    return Container(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      padding: const EdgeInsets.all(AppSpacing.md),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  ride.companyName ?? 'PickMeUp ride',
                  style: theme.textTheme.titleMedium,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              if (isPaid)
                const _StatusPill(
                  label: 'Paid',
                  colour: AppColors.toggleOn,
                  icon: Icons.check_circle,
                )
              else
                const _StatusPill(
                  label: 'Unpaid',
                  colour: AppColors.danger,
                  icon: Icons.schedule,
                ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          _Line(
            icon: Icons.my_location,
            colour: AppColors.toggleOn,
            text: ride.pickupAddress ?? '',
          ),
          if ((ride.destinationAddress ?? '').isNotEmpty)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.xs),
              child: _Line(
                icon: Icons.flag,
                colour: AppColors.danger,
                text: ride.destinationAddress!,
              ),
            ),
          if (ride.driverName != null)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.xs),
              child: _Line(
                icon: Icons.person_outline,
                colour: AppColors.textMuted,
                text: ride.driverName!,
              ),
            ),
          const Divider(height: AppSpacing.lg),
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Total to pay', style: theme.textTheme.bodySmall),
                    Text(
                      Formats.money(
                        card.payment?.amountCharged ?? ride.fare,
                        card.payment?.currency ?? 'BAM',
                      ),
                      style: theme.textTheme.headlineSmall,
                    ),
                  ],
                ),
              ),
              if (ride.distanceKm != null)
                Text(
                  Formats.distance(ride.distanceKm),
                  style: theme.textTheme.bodySmall,
                ),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          if (isPaid)
            _PaidFooter(paidAt: card.payment?.paidAt)
          else
            FilledButton(
              onPressed: isBusy || isBlocked ? null : onPay,
              child: isBusy
                  ? Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const SizedBox(
                          height: 18,
                          width: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        Text(
                          card.stage == _RideStage.confirming
                              ? 'CONFIRMING…'
                              : 'OPENING…',
                        ),
                      ],
                    )
                  : const Text('PAY WITH CARD'),
            ),
          if (card.stage == _RideStage.confirming)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.sm),
              child: Text(
                'Waiting for the bank to confirm. The ride is marked paid only '
                'once the confirmation reaches the server.',
                style: theme.textTheme.bodySmall,
              ),
            ),
        ],
      ),
    );
  }
}

class _PaidFooter extends StatelessWidget {
  const _PaidFooter({required this.paidAt});

  final DateTime? paidAt;

  @override
  Widget build(BuildContext context) {
    final when = paidAt?.toLocal();
    final stamp = when == null
        ? ''
        : ' on ${when.day}.${when.month}.${when.year}.';

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: AppSpacing.sm,
      ),
      decoration: BoxDecoration(
        color: AppColors.toggleOn.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        children: [
          const Icon(Icons.verified, color: AppColors.toggleOn, size: 18),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              'Paid$stamp Nothing more is owed for this ride.',
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        ],
      ),
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({
    required this.label,
    required this.colour,
    required this.icon,
  });

  final String label;
  final Color colour;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: colour.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: colour),
          const SizedBox(width: 6),
          Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: colour,
            ),
          ),
        ],
      ),
    );
  }
}

class _Line extends StatelessWidget {
  const _Line({required this.icon, required this.colour, required this.text});

  final IconData icon;
  final Color colour;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 15, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(
            text,
            style: Theme.of(context).textTheme.bodySmall,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(top: AppSpacing.xl),
      child: Column(
        children: [
          Container(
            width: 84,
            height: 84,
            decoration: const BoxDecoration(
              color: AppColors.sectionTint,
              shape: BoxShape.circle,
            ),
            child: const Icon(
              Icons.credit_card_off_outlined,
              size: 36,
              color: AppColors.mobileHeader,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          Text('Nothing to pay', style: theme.textTheme.headlineSmall),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'When a ride finishes, it shows up here so you can pay for it.',
            textAlign: TextAlign.center,
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}

class _GatewayNotice extends StatelessWidget {
  const _GatewayNotice({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Icon(Icons.info_outline, size: 18, color: AppColors.textMuted),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              message,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        ],
      ),
    );
  }
}

class _ErrorNotice extends StatelessWidget {
  const _ErrorNotice({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Text(
        message,
        style: TextStyle(color: Theme.of(context).colorScheme.error, fontSize: 13),
      ),
    );
  }
}
