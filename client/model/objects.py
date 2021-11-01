from __future__ import annotations

import abc
from typing import Any, Dict, List, Optional, Type, TypeVar

from . import header, utils

T = TypeVar('T', bound='BaseObject')

class BaseObject(abc.ABC):
    _attrs: Dict[str, Any]

    def __init__(self, **kwargs):
        self._attrs = {}
        self._attrs.update(kwargs)

    @classmethod
    def from_message(cls: Type[T], message: header.BaseMessage) -> T:
        ret = super().__new__(cls)
        ret.__init__(**message.content)
        return ret

    def update(self, message: header.BaseMessage) -> None:
        self._attrs.update(message.content)


class NaturalResourceCell(BaseObject):

    @staticmethod
    def clamp(val: int, min_: int, max_: int) -> int:
        if min_ > max_:
            raise ValueError('min > max')
        if (min_ > val):
            return min_
        if (max_ < val):
            return max_
        return val

    @classmethod
    def from_pos(cls, pos: utils.Vector) -> 'NaturalResourceCell':
        rowID = cls.clamp(int(pos.x / 33.75 + 256), 0, 511)
        columnID = cls.clamp(int(pos.z / 33.75 + 256), 0, 511)
        ret = cls(rowID=rowID, columnID=columnID)
        ret.update(ret.fetch_data())
        return ret
        
    @abc.abstractmethod
    def fetch_data(self) -> header.BaseMessage:
        ...

    @property
    def rowID(self) -> int:
        return self._attrs['rowID']

    @property
    def columnID(self) -> int:
        return self._attrs['columnID']

    @property
    def water(self) -> int:
        return self._attrs['water']

    @property
    def pollution(self) -> int:
        return self._attrs['pollution']

    @pollution.setter
    @abc.abstractmethod
    def pollution(self, pollution: int) -> None:
        ...

    @property
    def fertility(self) -> int:
        return self._attrs['fertility']

    @fertility.setter
    @abc.abstractmethod
    def fertility(self, fertility: int) -> None:
        ...
    
    @property
    def forest(self) -> int:
        return self._attrs['forest']

    @forest.setter
    @abc.abstractmethod
    def forest(self, forest: int) -> None:
        ...

    @property
    def ore(self) -> int:
        return self._attrs['ore']

    @ore.setter
    @abc.abstractmethod
    def ore(self, ore: int) -> None:
        ...

    @property
    def oil(self) -> int:
        return self._attrs['oil']

    @oil.setter
    @abc.abstractmethod
    def oil(self, oil: int) -> None:
        ...

    @property
    def id_(self) -> int:
        return self.rowID * 512 + self.columnID

class Point(utils.IPositionable):

    def __init__(self, vector: utils.Vector):
        self._vector = vector

    @property
    def type(self) -> str:
        return 'point'

    @property
    def position(self) -> utils.Vector:
        return self._vector
    
    @property
    @abc.abstractmethod
    def resources(self) -> NaturalResourceCell:
        return NaturalResourceCell.from_pos(self._vector)


class RenderableObjectHandle(BaseObject):

    @property
    def id_(self) -> int:
        return self._attrs['id']

    @abc.abstractmethod
    def delete(self) -> None:
        ...

class GameObject(utils.IPositionable, BaseObject):

    @property
    def id_(self) -> int:
        return self._attrs['id']

    @property
    def position(self) -> utils.Vector:
        return self._attrs['position']

    @property
    @abc.abstractmethod
    def type(self) -> str:
        ...


class EntityObject(GameObject):

    @property
    def exists(self) -> bool:
        return self._attrs['exists']

    @property
    def deleted(self) -> bool:
        return not self.exists

    @property
    def position(self) -> utils.Vector:
        return self._attrs['position']

    @position.setter
    def position(self, pos: utils.Vector) -> None:
        self.move(pos)

    @property
    def prefab_name(self) -> str:
        return self._attrs['prefab_name']

    @abc.abstractmethod
    def delete(self):
        ...

    @abc.abstractmethod
    def move(self, pos: utils.IPositionable):
        ...

    @abc.abstractmethod
    def refresh(self):
        ...


class RotatableEntity(EntityObject):

    @property
    def angle(self) -> float:
        return self._attrs['angle']

    @angle.setter
    def angle(self, angle: float) -> None:
        self.move(self.position, angle)

    @abc.abstractmethod
    def move(self, pos: utils.IPositionable, angle: float):
        ...


class NetPrefab(BaseObject):

    @property
    def type(self) -> str:
        return "net prefab"

    @property
    def id_(self) -> str:
        return self._attrs['id']

    @property
    def name(self) -> str:
        return self.id_

    @property
    def width(self) -> float:
        return self._attrs['width']

    @property
    def is_overground(self) -> bool:
        return self._attrs['is_overground']

    @property
    def is_underground(self) -> bool:
        return self._attrs['is_underground']

    @property
    def fw_vehicle_lane_count(self) -> int:
        return self._attrs['fw_vehicle_lane_count']

    @property
    def bw_vehicle_lane_count(self) -> int:
        return self._attrs['bw_vehicle_lane_count']


class NetworkObject(EntityObject):

    @property
    @abc.abstractmethod
    def prefab(self) -> NetPrefab:
        ...


class Tree(EntityObject):

    @property
    def type(self) -> str:
        return 'tree'


class Building(RotatableEntity):

    @property
    def type(self) -> str:
        return 'building'


class Prop(RotatableEntity):

    @property
    def type(self) -> str:
        return 'prop'


class Node(NetworkObject):

    @property
    def type(self) -> str:
        return 'node'

    @property
    def terrain_offset(self) -> float:
        return self._attrs['terrain_offset']

    @property
    def building_id(self) -> int:
        return self._attrs['building_id']

    @property
    def seg_count(self) -> int:
        return self._attrs['seg_count']

    @property
    @abc.abstractmethod
    def building(self) -> Building:
        ...

    @property
    @abc.abstractmethod
    def segments(self) -> List['Segment']:
        ...


class Segment(NetworkObject):

    @property
    def type(self) -> str:
        return 'segment'

    @property
    def start_node_id(self) -> int:
        return self._attrs['start_node_id']

    @property
    def end_node_id(self) -> int:
        return self._attrs['end_node_id']

    @property
    def start_dir(self) -> utils.Vector:
        return self._attrs['start_dir']

    @property
    def end_dir(self) -> utils.Vector:
        return self._attrs['end_dir']

    @property
    def bezier(self) -> utils.Bezier:
        return self._attrs['bezier']

    @property
    def length(self) -> float:
        return self._attrs['length']

    @property
    def is_straight(self) -> bool:
        return self._attrs['is_straight']

    @property
    @abc.abstractmethod
    def start_node(self) -> Node:
        ...

    @property
    @abc.abstractmethod
    def end_node(self) -> Node:
        ...

    def get_other_node(self, node: Node | int) -> Node:
        if isinstance(node, Node):
            node = node.id_
        if node == self.start_node_id:
            return self.end_node
        else:
            return self.start_node

class PathBuilder():

    def __init__(self,
        start_node: utils.IPositionable,
        options: Optional[str | utils.NetOptions] = None
    ):
        if options is None:
            if isinstance(start_node, Node):
                options = start_node.prefab_name
            else:
                raise ValueError(
                    "You must provide prefab name or NetOptions as the second "
                    "param, if that connot be inferred from first param"
                )

        self.options = utils.NetOptions.ensure(options)
        self._last_position = start_node
        self._last_segments: List[Segment] = []
        self._segments: List[Segment] = []

    @property
    def last_segments(self) -> List[Segment]:
        return self._last_segments

    @property
    def segments(self) -> List[Segment]:
        return self._segments
        self._last_position = value

    @property
    def first_node(self) -> Node:
        return self.segments[0].start_node

    @property
    def last_node(self) -> Node:
        return self.segments[-1].end_node

    @abc.abstractmethod
    def _create_segments(self,
        end_node: utils.IPositionable,
        options: Any, start_dir: Optional[utils.Vector],
        end_dir: Optional[utils.Vector], middle_pos: Optional[utils.Vector]
    ) -> List[Segment]:
        ...

    def _create_path(
        self, end_node: utils.IPositionable, options: Any,
        start_dir: Optional[utils.Vector], end_dir: Optional[utils.Vector],
        middle_pos: Optional[utils.Vector]
    ):
        self.last_segments.clear()
        self.last_segments.extend(self._create_segments(
            end_node, options, start_dir, end_dir, middle_pos
        ))
        self.segments.extend(self.last_segments)

    def path_to(self,
        end_node: utils.IPositionable, *,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None,
        middle_pos: Optional[utils.Vector] = None
    ):
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (middle_pos is None) == (start_dir is None):
            raise ValueError("middle_pos and start_dir can't be both present")
        
        self._create_path(
            end_node, self.options, start_dir, end_dir, middle_pos
        )
