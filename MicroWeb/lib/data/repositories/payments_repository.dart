import '../models/requests/create_payment_request.dart';
import '../models/requests/create_verification_request.dart';

/// Abstraction for Payments service REST calls routed via API Gateway base URL.
abstract class PaymentsRepository {
  Future<dynamic> listPayments({Map<String, dynamic>? query});
  Future<dynamic> getPaymentById(String id);
  Future<dynamic> createPayment(CreatePaymentRequest request);
  Future<dynamic> createPaymentVerification(CreateVerificationRequest request);
}
