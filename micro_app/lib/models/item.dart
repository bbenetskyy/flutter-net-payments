// Public enum as required
enum VerificationStatus { Pending, Rejected, Completed }

// Public enum for action as required
enum VerificationAction {
  NewUserCreated,
  UserAssignedToCard,
  CardPrinting,
  CardTermination,
  PaymentCreated,
  PaymentReverted,
}

class Item {
  final dynamic id;
  final String name;

  // Optional details commonly returned by APIs (e.g., verifications)
  final VerificationAction? action;
  final String? targetId;
  final VerificationStatus? status;
  final String? code;
  final String? createdBy;
  final String? assigneeUserId;
  final DateTime? createdAt;
  final DateTime? decidedAt;

  Item({
    required this.id,
    required this.name,
    this.action,
    this.targetId,
    this.status,
    this.code,
    this.createdBy,
    this.assigneeUserId,
    this.createdAt,
    this.decidedAt,
  });

  // Convenience label for UI
  String get statusLabel => status?.name ?? '';
  String get actionLabel => action?.name ?? '';

  factory Item.fromJson(Map<String, dynamic> json) {
    final dynamic id = json['id'] ?? json['userId'] ?? json['cardId'] ?? json['paymentId'];
    String name = '';
    for (final key in ['name', 'email', 'title', 'number', 'cardNumber', 'paymentNumber', 'description']) {
      final v = json[key];
      if (v != null && v.toString().isNotEmpty) {
        name = v.toString();
        break;
      }
    }
    if (name.isEmpty && id != null) {
      name = 'Item $id';
    }

    DateTime? _parseDate(dynamic v) {
      if (v == null) return null;
      try {
        return DateTime.parse(v.toString());
      } catch (_) {
        return null;
      }
    }

    VerificationStatus? _parseStatus(dynamic v) {
      if (v == null) return null;
      // Try numeric mapping: 0=Pending, 1=Rejected, 2=Completed
      if (v is num) {
        switch (v.toInt()) {
          case 0:
            return VerificationStatus.Pending;
          case 1:
            return VerificationStatus.Rejected;
          case 2:
            return VerificationStatus.Completed;
        }
      }
      // Try string mapping by name (case-insensitive)
      final s = v.toString().trim();
      if (s.isEmpty) return null;
      switch (s.toLowerCase()) {
        case 'pending':
          return VerificationStatus.Pending;
        case 'rejected':
          return VerificationStatus.Rejected;
        case 'completed':
          return VerificationStatus.Completed;
      }
      return null;
    }

    VerificationAction? _parseAction(dynamic v) {
      if (v == null) return null;
      if (v is num) {
        switch (v.toInt()) {
          case 0:
            return VerificationAction.NewUserCreated;
          case 1:
            return VerificationAction.UserAssignedToCard;
          case 2:
            return VerificationAction.CardPrinting;
          case 3:
            return VerificationAction.CardTermination;
          case 4:
            return VerificationAction.PaymentCreated;
          case 5:
            return VerificationAction.PaymentReverted;
        }
      }
      final s = v.toString().trim();
      if (s.isEmpty) return null;
      switch (s.toLowerCase()) {
        case 'newusercreated':
        case 'new_user_created':
        case 'new user created':
          return VerificationAction.NewUserCreated;
        case 'userassignedtocard':
        case 'user_assigned_to_card':
        case 'user assigned to card':
          return VerificationAction.UserAssignedToCard;
        case 'cardprinting':
        case 'card_printing':
        case 'card printing':
          return VerificationAction.CardPrinting;
        case 'cardtermination':
        case 'card_termination':
        case 'card termination':
          return VerificationAction.CardTermination;
        case 'paymentcreated':
        case 'payment_created':
        case 'payment created':
          return VerificationAction.PaymentCreated;
        case 'paymentreverted':
        case 'payment_reverted':
        case 'payment reverted':
          return VerificationAction.PaymentReverted;
      }
      return null;
    }

    return Item(
      id: id,
      name: name,
      action: _parseAction(json['action']),
      targetId: json['targetId']?.toString(),
      status: _parseStatus(json['status']),
      code: json['code']?.toString(),
      createdBy: json['createdBy']?.toString(),
      assigneeUserId: json['assigneeUserId']?.toString(),
      createdAt: _parseDate(json['createdAt']),
      decidedAt: _parseDate(json['decidedAt']),
    );
  }
}
