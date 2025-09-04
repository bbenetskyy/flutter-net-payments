//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'assign_card_request.g.dart';


@JsonSerializable(
  checked: true,
  createToJson: true,
  disallowUnrecognizedKeys: false,
  explicitToJson: true,
)
class AssignCardRequest {
  /// Returns a new [AssignCardRequest] instance.
  AssignCardRequest({

     this.userId,
  });

  @JsonKey(
    
    name: r'userId',
    required: false,
    includeIfNull: false,
  )


  final String? userId;





    @override
    bool operator ==(Object other) => identical(this, other) || other is AssignCardRequest &&
      other.userId == userId;

    @override
    int get hashCode =>
        userId.hashCode;

  factory AssignCardRequest.fromJson(Map<String, dynamic> json) => _$AssignCardRequestFromJson(json);

  Map<String, dynamic> toJson() => _$AssignCardRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }

}

