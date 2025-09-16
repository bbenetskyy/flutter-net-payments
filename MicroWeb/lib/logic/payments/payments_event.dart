import 'dart:async';

import 'package:equatable/equatable.dart';

import '../../data/models/requests/create_payment_request.dart';
import '../../data/models/responses/payment_response.dart';

abstract class PaymentsEvent extends Equatable {
  const PaymentsEvent();
  @override
  List<Object?> get props => [];
}

class PaymentsRequested extends PaymentsEvent {
  const PaymentsRequested({this.query});
  final Map<String, dynamic>? query;
}

class PaymentsPrefetchRequested extends PaymentsEvent {
  const PaymentsPrefetchRequested();
}

class BeneficiaryAccountsRequested extends PaymentsEvent {
  const BeneficiaryAccountsRequested(this.userId);
  final String? userId;
}

class PaymentCreateRequested extends PaymentsEvent {
  const PaymentCreateRequested(this.request, this.completer);
  final CreatePaymentRequest request;
  final Completer<PaymentResponse?> completer;
}

class PaymentRevertRequested extends PaymentsEvent {
  const PaymentRevertRequested(this.paymentId, this.completer);
  final String paymentId;
  final Completer<void> completer;
}
