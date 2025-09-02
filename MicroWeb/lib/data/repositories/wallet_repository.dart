import '../models/create_account_request.dart';

/// Abstraction for Wallet/Accounts service REST calls routed via API Gateway base URL.
abstract class WalletRepository {
  // Accounts CRUD
  Future<dynamic> listAccounts();
  Future<dynamic> getAccountById(String id);
  Future<dynamic> createAccount(CreateAccountRequest request);
  Future<void> deleteAccount(String id);

  // Account info/actions
  Future<dynamic> getBalance(String id);
  Future<void> deposit(String id, Map<String, dynamic> requestBody);
  Future<void> withdraw(String id, Map<String, dynamic> requestBody);
  Future<dynamic> listTransactions(String id, {Map<String, dynamic>? query});
}
