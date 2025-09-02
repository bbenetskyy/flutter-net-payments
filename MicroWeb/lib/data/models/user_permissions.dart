// UserPermissions flags matching backend [Flags] enum values.
// Provides helpers to convert between bitmask integer and Set<UserPermissions>.

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

enum UserPermissions {
  @JsonValue(0)
  None(0),
  @JsonValue(1)
  ViewPayments(1),
  @JsonValue(2)
  CreatePayments(2),
  @JsonValue(4)
  ConfirmPayments(4),
  @JsonValue(8)
  ViewUsers(8),
  @JsonValue(16)
  ManageCompanyUsers(16),
  @JsonValue(32)
  EditCompanyDetails(32),
  @JsonValue(64)
  ViewCards(64),
  @JsonValue(128)
  ManageCompanyCards(128);

  const UserPermissions(this.bit);

  final int bit;

  bool get isPowerOfTwo => bit != 0 && (bit & (bit - 1)) == 0;
}

extension UserPermissionsFlagSet on Set<UserPermissions> {
  int toBitMask() => fold(0, (acc, e) => acc | e.bit);
}

class UserPermissionsConverter {
  static Set<UserPermissions> fromBitMask(int? mask) {
    if (mask == null) return <UserPermissions>{};
    final set = <UserPermissions>{};
    for (final perm in UserPermissions.values) {
      if (perm.bit == 0) continue; // skip None in combinations
      if ((mask & perm.bit) == perm.bit) set.add(perm);
    }
    if (mask == 0) set.add(UserPermissions.None);
    return set;
  }

  static int toBitMask(Set<UserPermissions>? set) {
    if (set == null || set.isEmpty) return 0;
    return set.toBitMask();
  }
}
