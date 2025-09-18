import '../models/requests/create_account_request.dart';
import '../models/responses/wallet_response.dart';
import '../models/responses/ledger_response.dart';
import '../models/top_up_request.dart';

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

  // Wallets
  Future<WalletResponse> getWalletByUserId(String userId);
  Future<List<LedgerResponse>> getLedgerByUserId(String userId);
  Future<dynamic> topUp(String userId, TopUpRequest request);
}
