import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../providers/review_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../widgets/star_rating_input.dart';

class ReviewFormScreen extends StatefulWidget {
  const ReviewFormScreen({
    super.key,
    required this.ride,
    required this.canReviewCompany,
    required this.canReviewDriver,
  });

  final Order ride;
  final bool canReviewCompany;
  final bool canReviewDriver;

  static Future<bool?> open(
    BuildContext context, {
    required Order ride,
    required bool canReviewCompany,
    required bool canReviewDriver,
  }) {
    return Navigator.of(context).push<bool>(
      MaterialPageRoute<bool>(
        builder: (_) => ReviewFormScreen(
          ride: ride,
          canReviewCompany: canReviewCompany,
          canReviewDriver: canReviewDriver,
        ),
      ),
    );
  }

  @override
  State<ReviewFormScreen> createState() => _ReviewFormScreenState();
}

class _ReviewFormScreenState extends State<ReviewFormScreen> {
  static const int _maxCommentLength = 1000;

  final _commentController = TextEditingController();
  final _reviews = ReviewProvider();

  int _rating = 0;
  late bool _reviewCompany = widget.canReviewCompany;
  late bool _reviewDriver = widget.canReviewDriver;
  bool _isSubmitting = false;
  String? _ratingError;
  String? _targetError;
  String? _submitError;

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _ratingError = _rating == 0 ? 'Choose a rating from 1 to 5 stars.' : null;
      _targetError = !_reviewCompany && !_reviewDriver
          ? 'Choose who you are reviewing.'
          : null;
      _submitError = null;
    });

    if (_ratingError != null || _targetError != null) {
      return;
    }

    setState(() => _isSubmitting = true);

    final comment = _commentController.text.trim();
    final navigator = Navigator.of(context);

    try {
      await _reviews.insert({
        'orderId': widget.ride.id,
        'reviewCompany': _reviewCompany,
        'reviewDriver': _reviewDriver,
        'rating': _rating,
        'comment': comment.isEmpty ? null : comment,
      });

      navigator.pop(true);
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _submitError = e.message;
        _isSubmitting = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _submitError =
            'Could not reach the server. Check that the API is running.';
        _isSubmitting = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final ride = widget.ride;

    return Scaffold(
      appBar: AppBar(title: const Text('Leave a review')),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          Text(
            ride.companyName ?? 'Your ride',
            style: theme.textTheme.headlineSmall,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: AppSpacing.xs),
          Text(
            ride.destinationAddress == null
                ? ride.pickupAddress ?? ''
                : '${ride.pickupAddress ?? ''} → ${ride.destinationAddress}',
            style: theme.textTheme.bodySmall,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: AppSpacing.xl),
          Container(
            padding: const EdgeInsets.symmetric(vertical: AppSpacing.lg),
            decoration: BoxDecoration(
              color: AppColors.sectionTint,
              borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
            ),
            child: StarRatingInput(
              value: _rating,
              onChanged: (value) => setState(() {
                _rating = value;
                _ratingError = null;
              }),
            ),
          ),
          if (_ratingError != null)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.sm),
              child: Text(
                _ratingError!,
                style: TextStyle(
                  color: theme.colorScheme.error,
                  fontSize: 12,
                ),
              ),
            ),
          const SizedBox(height: AppSpacing.lg),
          Text('Who are you reviewing?', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.xs),
          if (widget.canReviewCompany)
            CheckboxListTile(
              contentPadding: EdgeInsets.zero,
              controlAffinity: ListTileControlAffinity.leading,
              value: _reviewCompany,
              onChanged: _isSubmitting
                  ? null
                  : (value) => setState(() {
                      _reviewCompany = value ?? false;
                      _targetError = null;
                    }),
              title: Text(ride.companyName ?? 'The company'),
              subtitle: const Text('The taxi company'),
            ),
          if (widget.canReviewDriver)
            CheckboxListTile(
              contentPadding: EdgeInsets.zero,
              controlAffinity: ListTileControlAffinity.leading,
              value: _reviewDriver,
              onChanged: _isSubmitting
                  ? null
                  : (value) => setState(() {
                      _reviewDriver = value ?? false;
                      _targetError = null;
                    }),
              title: Text(ride.driverName ?? 'The driver'),
              subtitle: const Text('The driver who came'),
            ),
          if (_targetError != null)
            Text(
              _targetError!,
              style: TextStyle(color: theme.colorScheme.error, fontSize: 12),
            ),
          const SizedBox(height: AppSpacing.lg),
          TextField(
            controller: _commentController,
            maxLines: 4,
            maxLength: _maxCommentLength,
            enabled: !_isSubmitting,
            decoration: const InputDecoration(
              hintText: 'Say a little about the ride (optional)',
              alignLabelWithHint: true,
            ),
          ),
          if (_submitError != null)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.sm),
              child: Text(
                _submitError!,
                style: TextStyle(color: theme.colorScheme.error, fontSize: 13),
              ),
            ),
          const SizedBox(height: AppSpacing.sm),
          FilledButton(
            onPressed: _isSubmitting ? null : _submit,
            child: _isSubmitting
                ? const SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Text('SUBMIT REVIEW'),
          ),
        ],
      ),
    );
  }
}
